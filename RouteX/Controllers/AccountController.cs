using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;
using Microsoft.AspNetCore.Http;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User model)
        {
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
                lockoutOnFailure: false);
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
                    if (customUser.Role == "OperationsStaff")
                    {
                        return RedirectToAction("OpStaffDashboard", "Home");
                    }

                    if (customUser.Role == "Finance" || customUser.Role == "Admin")
                    {
                        return RedirectToAction("FinanceDashboard", "Home");
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid email or password. Please try again.";
                return View("LoginPage", model);
            }
        }

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

