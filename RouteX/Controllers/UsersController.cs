using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;
using System.Collections.Generic;
using System.Linq;

namespace RouteX.Controllers
{
    
    public class UsersController : Controller
    {
        private static readonly HashSet<string> ProtectedEmails = new(StringComparer.OrdinalIgnoreCase)
        {
            "superadmin@routex.com",
            "admin@routex.com",
            "operationstaff@routex.com",
            "finance@routex.com"
        };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditService _auditService;

        public UsersController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET: Users
        public async Task<IActionResult> UsersPage()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.Status != UserStatus.Archived.ToString())
                .OrderByDescending(u => u.UserId)
                .ToListAsync();

            return View(users);
        }

        // POST: Users/BackfillUserPasswords
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BackfillUserPasswords()
        {
            const string tempPassword = "Temp@1234";

            var usersNeedingPasswords = await _context.Users
                .Where(u => string.IsNullOrWhiteSpace(u.Password))
                .ToListAsync();

            if (usersNeedingPasswords.Count == 0)
            {
                TempData["Success"] = "No users require password backfill.";
                return RedirectToAction(nameof(UsersPage));
            }

            var errors = new List<string>();
            var updatedCount = 0;

            foreach (var user in usersNeedingPasswords)
            {
                var identityUser = await _userManager.FindByEmailAsync(user.Email);
                if (identityUser == null)
                {
                    identityUser = new IdentityUser
                    {
                        UserName = user.Email,
                        Email = user.Email,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(identityUser, tempPassword);
                    if (!createResult.Succeeded)
                    {
                        errors.AddRange(createResult.Errors.Select(e => e.Description));
                        continue;
                    }
                }
                else
                {
                    var hasPassword = await _userManager.HasPasswordAsync(identityUser);
                    if (hasPassword)
                    {
                        var removeResult = await _userManager.RemovePasswordAsync(identityUser);
                        if (!removeResult.Succeeded)
                        {
                            errors.AddRange(removeResult.Errors.Select(e => e.Description));
                            continue;
                        }
                    }

                    var addPasswordResult = await _userManager.AddPasswordAsync(identityUser, tempPassword);
                    if (!addPasswordResult.Succeeded)
                    {
                        errors.AddRange(addPasswordResult.Errors.Select(e => e.Description));
                        continue;
                    }
                }

                identityUser = await _userManager.FindByEmailAsync(user.Email);
                user.Password = identityUser?.PasswordHash ?? string.Empty;
                updatedCount++;
            }

            await _context.SaveChangesAsync();

            if (errors.Count > 0)
            {
                TempData["Error"] = $"Backfill completed with errors. Updated {updatedCount} user(s).";
            }
            else
            {
                TempData["Success"] = $"Backfilled passwords for {updatedCount} user(s).";
            }

            return RedirectToAction(nameof(UsersPage));
        }

        // GET: Users/AddUser
        public async Task<IActionResult> AddUser()
        {
            ViewData["Title"] = "Add User";

            // Get active roles from RolesController sample data
            var activeRoles = GetActiveRoles();
            ViewBag.ActiveRoles = activeRoles;

            // Get active branches for dropdown
            var activeBranches = await _context.Branches
                .Where(b => !b.IsArchived && b.Status == BranchStatus.Active)
                .OrderBy(b => b.BranchName)
                .ToListAsync();
            ViewBag.ActiveBranches = activeBranches;

            var viewModel = new CreateUserViewModel
            {
                Status = UserStatus.Active.ToString(),
                IsEditMode = false
            };

            return View(viewModel);
        }

        // POST: Users/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(CreateUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var existingIdentity = await _userManager.FindByEmailAsync(viewModel.Email);
                if (existingIdentity != null || await _context.Users.AnyAsync(u => u.Email == viewModel.Email))
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                }
                else
                {
                    var identityUser = new IdentityUser
                    {
                        UserName = viewModel.Email,
                        Email = viewModel.Email,
                        EmailConfirmed = true
                    };

                    // Create user with hashed password
                    var createResult = await _userManager.CreateAsync(identityUser, viewModel.Password);
                    if (createResult.Succeeded)
                    {
                        // Refresh the identity user to get the hashed password
                        identityUser = await _userManager.FindByEmailAsync(viewModel.Email);
                        if (identityUser != null && !string.IsNullOrEmpty(identityUser.PasswordHash))
                        {
                            var passwordHash = identityUser.PasswordHash;
                            
                            // DEBUG: Log the hash length and first few characters
                            System.Diagnostics.Debug.WriteLine($"Password hash length: {passwordHash.Length}");
                            System.Diagnostics.Debug.WriteLine($"Password hash starts: {passwordHash.Substring(0, Math.Min(50, passwordHash.Length))}...");
                            
                            // Create custom user record with hashed password
                            var customUser = new User
                            {
                                FirstName = viewModel.FirstName,
                                LastName = viewModel.LastName,
                                Email = viewModel.Email,
                                Password = passwordHash, // Store hashed password
                                Role = viewModel.Role,
                                Status = viewModel.Status,
                                BranchId = viewModel.BranchId // Assign branch
                            };
                            
                            _context.Users.Add(customUser);
                            await _context.SaveChangesAsync();

                            var actingUser = HttpContext.Session.GetString("UserEmail") ?? "System";
                            await _auditService.LogActionAsync(actingUser, $"Create:User:{customUser.UserId}");
                            
                            // DEBUG: Verify it was saved
                            System.Diagnostics.Debug.WriteLine($"Custom user created with ID: {customUser.UserId}");

                            TempData["Success"] = "User created successfully!";
                            return RedirectToAction(nameof(UsersPage));
                        }
                        else
                        {
                            // DEBUG: Log what went wrong
                            System.Diagnostics.Debug.WriteLine($"Identity user is null: {identityUser == null}");
                            System.Diagnostics.Debug.WriteLine($"PasswordHash is null or empty: {string.IsNullOrEmpty(identityUser?.PasswordHash)}");
                            ModelState.AddModelError("", "Error: Password was not hashed properly. Please try again.");
                        }
                    }

                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            ViewBag.ActiveRoles = GetActiveRoles();
            return View(viewModel);
        }

        private List<Role> GetActiveRoles()
        {
            // Fixed roles as specified
            return new List<Role>
            {
                new Role { RoleId = 1, RoleName = "SuperAdmin", Status = UserStatus.Active.ToString(), Description = "Full system administration with all privileges" },
                new Role { RoleId = 2, RoleName = "Admin", Status = UserStatus.Active.ToString(), Description = "System administration with user management privileges" },
                new Role { RoleId = 3, RoleName = "OperationsStaff", Status = UserStatus.Active.ToString(), Description = "Vehicle operations and dispatch management" },
                new Role { RoleId = 4, RoleName = "Finance", Status = UserStatus.Active.ToString(), Description = "Financial reporting and expense management" }
            };
        }

        // GET: Users/EditUser/5
        public async Task<IActionResult> EditUser(int id)
        {
            ViewData["Title"] = "Edit User";
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(UsersPage));
            }

            var viewModel = new EditUserViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                Password = string.Empty, // Don't populate password in edit mode
                ConfirmPassword = string.Empty, // Don't populate confirm password
                UpdatePassword = false
            };

            ViewBag.ActiveRoles = GetActiveRoles();
            return View(viewModel);
        }

        // POST: Users/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == viewModel.UserId);
                if (existingUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(UsersPage));
                }

                var identityUser = await _userManager.FindByEmailAsync(existingUser.Email);
                if (identityUser == null)
                {
                    identityUser = await _userManager.FindByEmailAsync(viewModel.Email);
                }

                if (identityUser != null)
                {
                    // Update email if changed
                    if (identityUser.Email != viewModel.Email)
                    {
                        identityUser.Email = viewModel.Email;
                        identityUser.UserName = viewModel.Email;
                        var updateResult = await _userManager.UpdateAsync(identityUser);
                        if (!updateResult.Succeeded)
                        {
                            foreach (var error in updateResult.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            ViewBag.ActiveRoles = GetActiveRoles();
                            return View(viewModel);
                        }
                    }

                    // Update password only if provided
                    if (!string.IsNullOrWhiteSpace(viewModel.Password))
                    {
                        var removeResult = await _userManager.RemovePasswordAsync(identityUser);
                        if (removeResult.Succeeded)
                        {
                            var addPasswordResult = await _userManager.AddPasswordAsync(identityUser, viewModel.Password);
                            if (!addPasswordResult.Succeeded)
                            {
                                foreach (var error in addPasswordResult.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                                ViewBag.ActiveRoles = GetActiveRoles();
                                return View(viewModel);
                            }
                        }
                    }

                    // Get the updated hashed password
                    var refreshedIdentity = await _userManager.FindByEmailAsync(viewModel.Email);
                    var passwordHash = refreshedIdentity?.PasswordHash ?? identityUser.PasswordHash ?? string.Empty;
                    existingUser.Password = passwordHash;
                }

                // Update user details
                existingUser.FirstName = viewModel.FirstName;
                existingUser.LastName = viewModel.LastName;
                existingUser.Email = viewModel.Email;
                existingUser.Role = viewModel.Role;
                existingUser.Status = viewModel.Status;
                await _context.SaveChangesAsync();

                var actingUser = HttpContext.Session.GetString("UserEmail") ?? "System";
                await _auditService.LogActionAsync(actingUser, $"Update:User:{existingUser.UserId}");

                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(UsersPage));
            }

            ViewBag.ActiveRoles = GetActiveRoles();
            return View(viewModel);
        }

        // GET: Users/ViewUser/5
        public async Task<IActionResult> ViewUser(int id)
        {
            ViewData["Title"] = "View User";
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(UsersPage));
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (ProtectedEmails.Contains(user.Email))
            {
                return Json(new { success = false, message = "This account cannot be archived." });
            }

            var sql = "UPDATE Users SET Status = @Status WHERE UserId = @UserId";
            var parameters = new[]
            {
                new Microsoft.Data.SqlClient.SqlParameter("@Status", UserStatus.Archived.ToString()),
                new Microsoft.Data.SqlClient.SqlParameter("@UserId", user.UserId)
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);

            var archivedBy = HttpContext.Session.GetString("UserEmail") ?? "System";
            await _auditService.LogActionAsync(archivedBy, $"Archive:User:{user.UserId}:Status:{user.Status}");

            return Json(new { success = true, message = "User archived successfully." });
        }

        private Task ArchiveNonProtectedUsersAsync()
        {
            return Task.CompletedTask;
        }
    }
}
