using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace RouteX.Controllers

{

    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditService _auditService;
        public AccountController(

            ApplicationDbContext context,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IAuditService auditService)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult LoginPage()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            var role = HttpContext.Session.GetString("UserRole") ?? "";

            string dashboard = role switch
            {
                "SuperAdmin"      => "/Home/Index",
                "Admin"           => "/Home/Index",
                "Administrator"   => "/Home/Index",
                "Finance"         => "/Home/FinanceDashboard",
                "OperationsStaff" => "/Home/OpStaffDashboard",
                _                 => "/Home/Index"
            };

            return Redirect(dashboard);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(User model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Invalid login information.";
                return View("LoginPage", model);
            }

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.ErrorMessage = "Email and password are required.";
                return View("LoginPage", model);
            }

            var customUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (customUser != null && (customUser.Status == UserStatus.Inactive.ToString() || customUser.Status == UserStatus.Archived.ToString()))
            {
                ViewBag.ErrorMessage = "This account is inactive or archived. Please contact an administrator.";
                return View("LoginPage", model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: true);
            if (result.Succeeded)

            {

                var identityUser = await _userManager.FindByEmailAsync(model.Email);
                if (identityUser != null)
                {
                    if (customUser != null)
                    {
                        HttpContext.Session.SetString("UserEmail", customUser.Email);
                        HttpContext.Session.SetString("UserName", $"{customUser.FirstName} {customUser.LastName}");
                        HttpContext.Session.SetInt32("UserId", customUser.UserId);
                        HttpContext.Session.SetString("UserRole", customUser.Role);
                        
                        // Sync the custom role into Identity claims so [Authorize(Roles=...)] works
                        // Always force-sync to ensure the correct role is in the Identity cookie
                        var existingRoles = await _userManager.GetRolesAsync(identityUser);
                        if (!existingRoles.Contains(customUser.Role, StringComparer.OrdinalIgnoreCase))
                        {
                            if (existingRoles.Count > 0)
                                await _userManager.RemoveFromRolesAsync(identityUser, existingRoles);
                            // Ensure the role exists in Identity before assigning
                            var roleManager = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
                            if (!await roleManager.RoleExistsAsync(customUser.Role))
                            {
                                await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(customUser.Role));
                            }
                            await _userManager.AddToRoleAsync(identityUser, customUser.Role);
                        }
                        // Always refresh the sign-in cookie so role claims are current
                        await _signInManager.RefreshSignInAsync(identityUser);
                        
                        // Store branch information
                        if (customUser.BranchId.HasValue)
                        {
                            HttpContext.Session.SetInt32("UserBranchId", customUser.BranchId.Value);
                            var branch = _context.Branches.FirstOrDefault(b => b.BranchId == customUser.BranchId.Value);
                            if (branch != null)
                            {
                                HttpContext.Session.SetString("UserBranchName", branch.BranchName);
                            }
                        }
                        
                        // Log successful login
                        await _auditService.LogActionAsync(customUser.Email, "Login");
                    }
                    else
                    {
                        // Log successful login for Identity user without custom user
                        await _auditService.LogActionAsync(model.Email, "Login");
                    }
                }
                if (customUser != null)
                {
                    if (customUser.Role == "Admin" || customUser.Role == "Administrator")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    
                    if (customUser.Role == "Finance")
                    {
                        return RedirectToAction("FinanceDashboard", "Home");
                    }
                    
                    if (customUser.Role == "OperationsStaff")
                    {
                        return RedirectToAction("OpStaffDashboard", "Home");
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Log failed login attempt to audit log
                await _auditService.LogActionAsync(model.Email, $"FailedLogin:{(result.IsLockedOut ? "AccountLocked" : "InvalidCredentials")}");
                ViewBag.ErrorMessage = result.IsLockedOut
                    ? "Your account has been locked due to too many failed attempts. Please try again in 15 minutes."
                    : "Invalid email or password. Please try again.";
                return View("LoginPage", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            
            // Log logout if we have user information
            if (!string.IsNullOrEmpty(userEmail))
            {
                await _auditService.LogActionAsync(userEmail, "Logout");
            }
            
            return RedirectToAction("LoginPage");
        }
    }
}

