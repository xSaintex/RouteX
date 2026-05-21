using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "SuperAdmin")]
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
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IAuditService auditService, ILogger<UsersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
            _logger = logger;
        }

        private bool IsSuperAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            return userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
        }

        // GET: Users
        public async Task<IActionResult> UsersPage()
        {
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

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
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> BackfillUserPasswords()
        {
            // Get temporary password from configuration or generate a secure one
            var tempPassword = System.Guid.NewGuid().ToString() + "@Temp1";
            if (string.IsNullOrWhiteSpace(tempPassword))
            {
                TempData["Error"] = "Failed to generate temporary password.";
                return RedirectToAction(nameof(UsersPage));
            }

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
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

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
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ActiveRoles = GetActiveRoles();
                var activeBranches = await _context.Branches
                    .Where(b => !b.IsArchived && b.Status == BranchStatus.Active)
                    .OrderBy(b => b.BranchName)
                    .ToListAsync();
                ViewBag.ActiveBranches = activeBranches;
                return View(viewModel);
            }

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
                            
                            _logger.LogInformation("Custom user created with ID: {UserId}", customUser.UserId);

                            TempData["Success"] = "User created successfully!";
                            return RedirectToAction(nameof(UsersPage));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Error: Password was not hashed properly. Please try again.");
                        }
                    }

                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
            }

            // Repopulate ViewBags as ModelState is invalid
            ViewBag.ActiveRoles = GetActiveRoles();
            var activeBranchesForInvalid = await _context.Branches
                .Where(b => !b.IsArchived && b.Status == BranchStatus.Active)
                .OrderBy(b => b.BranchName)
                .ToListAsync();
            ViewBag.ActiveBranches = activeBranchesForInvalid;
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
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            // Allow empty passwords during edit — remove validation errors for blank password fields
            if (string.IsNullOrWhiteSpace(viewModel.Password))
            {
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ActiveRoles = GetActiveRoles();
                return View(viewModel);
            }

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

            // Prevent setting Status to Archived via edit form
            if (string.Equals(viewModel.Status, UserStatus.Archived.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                // Do not allow manually archiving a user from edit; keep existing status
                viewModel.Status = existingUser.Status;
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

        // GET: Users/ViewUser/5
        public async Task<IActionResult> ViewUser(int id)
        {
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
            if (!IsSuperAdmin())
            {
                return Json(new { success = false, message = "You do not have permission to archive users." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

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

    }
}
