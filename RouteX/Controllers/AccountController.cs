using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RouteX.Data;
using RouteX.Models;
using Microsoft.AspNetCore.Http;

namespace RouteX.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(
            ApplicationDbContext context,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: Account/LoginPage
        [HttpGet]
        public IActionResult LoginPage()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.ErrorMessage = "Email and password are required.";
                return View("LoginPage", model);
            }

            // Use Identity to sign in
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                isPersistent: false, 
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Get the user from Identity
                var identityUser = await _userManager.FindByEmailAsync(model.Email);

                if (identityUser != null)
                {
                    // Store additional info in session for compatibility
                    var customUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (customUser != null)
                    {
                        HttpContext.Session.SetString("UserEmail", customUser.Email);
                        HttpContext.Session.SetString("UserName", $"{customUser.FirstName} {customUser.LastName}");
                        HttpContext.Session.SetInt32("UserId", customUser.UserId);
                        HttpContext.Session.SetString("UserRole", customUser.Role);
                    }
                }

                // Redirect to home/dashboard
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid email or password. Please try again.";
                return View("LoginPage", model);
            }
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            // Sign out from Identity
            await _signInManager.SignOutAsync();

            // Clear session
            HttpContext.Session.Clear();

            // Redirect to login page
            return RedirectToAction("LoginPage");
        }
    }
}
