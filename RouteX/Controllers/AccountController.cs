using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace RouteX.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly IDataProtector _protector;
        private readonly IEmailService _emailService;

        public AccountController(
            ApplicationDbContext context,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IAuditService auditService,
            IDataProtectionProvider dataProtectionProvider,
            IEmailService emailService)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService;
            _protector = dataProtectionProvider.CreateProtector("RouteX.MfaToken");
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult LoginPage()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
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

            var identityUser = await _userManager.FindByEmailAsync(model.Email);
            if (identityUser == null)
            {
                ViewBag.ErrorMessage = "Invalid email or password. Please try again.";
                return View("LoginPage", model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, model.Password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                bool twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(identityUser);
                if (twoFactorEnabled)
                {
                    // Generate OTP
                    string otpCode = GenerateOtpCode();

                    // Send email via Gmail SMTP
                    string subject = "RouteX - Login Verification Code";
                    string body = $"<h3>RouteX Two-Factor Authentication</h3><p>Your 6-digit verification code is: <strong>{otpCode}</strong></p><p>This code will expire in 5 minutes.</p>";
                    try
                    {
                        await _emailService.SendEmailAsync(identityUser.Email!, subject, body);
                    }
                    catch (Exception)
                    {
                        ViewBag.ErrorMessage = "Failed to send verification email. Please check your SMTP configuration.";
                        return View("LoginPage", model);
                    }

                    // Encrypt stateless token
                    var expiryTime = DateTime.UtcNow.AddMinutes(5);
                    string payload = $"{otpCode}|{identityUser.Email}|{expiryTime.Ticks}";
                    string encryptedToken = _protector.Protect(payload);

                    ViewBag.Token = encryptedToken;
                    ViewBag.Email = identityUser.Email;
                    return View("Verify2fa");
                }

                // If 2FA not enabled, login directly
                await _signInManager.SignInAsync(identityUser, isPersistent: false);

                if (customUser != null)
                {
                    HttpContext.Session.SetString("UserEmail", customUser.Email);
                    HttpContext.Session.SetString("UserName", $"{customUser.FirstName} {customUser.LastName}");
                    HttpContext.Session.SetInt32("UserId", customUser.UserId);
                    HttpContext.Session.SetString("UserRole", customUser.Role);
                    
                    var existingRoles = await _userManager.GetRolesAsync(identityUser) ?? new List<string>();
                    if (!existingRoles.Contains(customUser.Role, StringComparer.OrdinalIgnoreCase))
                    {
                        if (existingRoles.Count > 0)
                            await _userManager.RemoveFromRolesAsync(identityUser, existingRoles);
                        var roleManager = HttpContext.RequestServices?.GetService<RoleManager<IdentityRole>>();
                        if (roleManager != null)
                        {
                            if (!await roleManager.RoleExistsAsync(customUser.Role))
                            {
                                await roleManager.CreateAsync(new IdentityRole(customUser.Role));
                            }
                            await _userManager.AddToRoleAsync(identityUser, customUser.Role);
                        }
                    }
                    await _signInManager.RefreshSignInAsync(identityUser);
                    
                    if (customUser.BranchId.HasValue)
                    {
                        HttpContext.Session.SetInt32("UserBranchId", customUser.BranchId.Value);
                        var branch = _context.Branches.FirstOrDefault(b => b.BranchId == customUser.BranchId.Value);
                        if (branch != null)
                        {
                            HttpContext.Session.SetString("UserBranchName", branch.BranchName);
                        }
                    }
                    
                    await _auditService.LogActionAsync(customUser.Email, "Login");
                }
                else
                {
                    await _auditService.LogActionAsync(model.Email, "Login");
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
                await _auditService.LogActionAsync(model.Email, $"FailedLogin:{(result.IsLockedOut ? "AccountLocked" : "InvalidCredentials")}");
                ViewBag.ErrorMessage = result.IsLockedOut
                    ? "Your account has been locked due to too many failed attempts. Please try again in 15 minutes."
                    : "Invalid email or password. Please try again.";
                return View("LoginPage", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2fa(string token, string code)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(code))
            {
                ViewBag.ErrorMessage = "Invalid request or missing verification code.";
                return View("LoginPage");
            }

            try
            {
                string decrypted = _protector.Unprotect(token);
                var parts = decrypted.Split('|');
                if (parts.Length < 3)
                {
                    ViewBag.ErrorMessage = "Invalid verification token format.";
                    return View("LoginPage");
                }

                string expectedOtp = parts[0];
                string email = parts[1];
                long ticks = long.Parse(parts[2]);
                var expiryTime = new DateTime(ticks, DateTimeKind.Utc);

                ViewBag.Token = token;
                ViewBag.Email = email;

                if (DateTime.UtcNow > expiryTime)
                {
                    ViewBag.ErrorMessage = "Verification code has expired. Please request a new code.";
                    return View("Verify2fa");
                }

                if (expectedOtp != code)
                {
                    ViewBag.ErrorMessage = "Incorrect verification code. Please try again.";
                    return View("Verify2fa");
                }

                var identityUser = await _userManager.FindByEmailAsync(email);
                if (identityUser == null)
                {
                    ViewBag.ErrorMessage = "User not found.";
                    return View("LoginPage");
                }

                var customUser = _context.Users.FirstOrDefault(u => u.Email == email);
                if (customUser != null && (customUser.Status == UserStatus.Inactive.ToString() || customUser.Status == UserStatus.Archived.ToString()))
                {
                    ViewBag.ErrorMessage = "This account is inactive or archived. Please contact an administrator.";
                    return View("LoginPage");
                }

                await _userManager.ResetAccessFailedCountAsync(identityUser);
                await _signInManager.SignInAsync(identityUser, isPersistent: false);

                if (customUser != null)
                {
                    HttpContext.Session.SetString("UserEmail", customUser.Email);
                    HttpContext.Session.SetString("UserName", $"{customUser.FirstName} {customUser.LastName}");
                    HttpContext.Session.SetInt32("UserId", customUser.UserId);
                    HttpContext.Session.SetString("UserRole", customUser.Role);

                    var existingRoles = await _userManager.GetRolesAsync(identityUser) ?? new List<string>();
                    if (!existingRoles.Contains(customUser.Role, StringComparer.OrdinalIgnoreCase))
                    {
                        if (existingRoles.Count > 0)
                            await _userManager.RemoveFromRolesAsync(identityUser, existingRoles);
                        var roleManager = HttpContext.RequestServices?.GetService<RoleManager<IdentityRole>>();
                        if (roleManager != null)
                        {
                            if (!await roleManager.RoleExistsAsync(customUser.Role))
                            {
                                await roleManager.CreateAsync(new IdentityRole(customUser.Role));
                            }
                            await _userManager.AddToRoleAsync(identityUser, customUser.Role);
                        }
                    }
                    await _signInManager.RefreshSignInAsync(identityUser);

                    if (customUser.BranchId.HasValue)
                    {
                        HttpContext.Session.SetInt32("UserBranchId", customUser.BranchId.Value);
                        var branch = _context.Branches.FirstOrDefault(b => b.BranchId == customUser.BranchId.Value);
                        if (branch != null)
                        {
                            HttpContext.Session.SetString("UserBranchName", branch.BranchName);
                        }
                    }

                    await _auditService.LogActionAsync(customUser.Email, "Login (2FA)");
                }
                else
                {
                    await _auditService.LogActionAsync(email, "Login (2FA)");
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
            catch (Exception)
            {
                ViewBag.ErrorMessage = "The verification session has expired or is invalid. Please login again.";
                return View("LoginPage");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Session expired. Please login again.";
                return RedirectToAction("LoginPage");
            }

            try
            {
                string decrypted = _protector.Unprotect(token);
                var parts = decrypted.Split('|');
                string email = parts[1];

                var identityUser = await _userManager.FindByEmailAsync(email);
                if (identityUser != null)
                {
                    string otpCode = GenerateOtpCode();
                    string subject = "RouteX - Your New Verification Code";
                    string body = $"<h3>RouteX Two-Factor Authentication</h3><p>Your new 6-digit verification code is: <strong>{otpCode}</strong></p><p>This code will expire in 5 minutes.</p>";

                    await _emailService.SendEmailAsync(identityUser.Email!, subject, body);

                    var expiryTime = DateTime.UtcNow.AddMinutes(5);
                    string payload = $"{otpCode}|{identityUser.Email}|{expiryTime.Ticks}";
                    string encryptedToken = _protector.Protect(payload);

                    ViewBag.Token = encryptedToken;
                    ViewBag.Email = identityUser.Email;
                    TempData["SuccessMessage"] = "A new verification code has been sent to your email.";
                    return View("Verify2fa");
                }
            }
            catch (Exception)
            {
            }

            ViewBag.ErrorMessage = "Failed to resend verification code. Please login again.";
            return View("LoginPage");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("LoginPage");
            }

            var customUser = _context.Users.FirstOrDefault(u => u.Email == email);
            if (customUser == null)
            {
                return NotFound();
            }

            var identityUser = await _userManager.FindByEmailAsync(email);
            bool twoFactorEnabled = identityUser != null && identityUser.TwoFactorEnabled;

            ViewBag.TwoFactorEnabled = twoFactorEnabled;
            return View(customUser);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, bool enable2Fa)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("LoginPage");
            }

            var customUser = _context.Users.FirstOrDefault(u => u.Email == email);
            if (customUser == null)
            {
                return NotFound();
            }

            customUser.FirstName = firstName;
            customUser.LastName = lastName;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", $"{firstName} {lastName}");

            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(identityUser, enable2Fa);
                await _userManager.UpdateSecurityStampAsync(identityUser);
            }

            TempData["SuccessMessage"] = "Profile settings updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();

            if (!string.IsNullOrEmpty(userEmail))
            {
                await _auditService.LogActionAsync(userEmail, "Logout");
            }

            return RedirectToAction("LoginPage");
        }

        private string GenerateOtpCode()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                uint val = BitConverter.ToUInt32(bytes, 0) % 1000000;
                return val.ToString("D6");
            }
        }
    }
}
