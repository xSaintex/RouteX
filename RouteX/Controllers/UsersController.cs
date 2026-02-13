using Microsoft.AspNetCore.Mvc;
using RouteX.Models;
using System.Collections.Generic;
using System.Linq;

namespace RouteX.Controllers
{
    
    public class UsersController : Controller
    {
        // GET: Users
        public IActionResult UsersPage()
        {
            var users = GetSampleUsers();
            return View(users);
        }

        // GET: Users/AddUser
        public IActionResult AddUser()
        {
            ViewData["Title"] = "Add User";
            
            // Get active roles from RolesController sample data
            var activeRoles = GetActiveRoles();
            ViewBag.ActiveRoles = activeRoles;
            
            return View();
        }

        // POST: Users/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(User user)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would save the user to the database here
                // For now, we'll just redirect back to UsersPage with success message
                TempData["Success"] = "User created successfully!";
                return RedirectToAction(nameof(UsersPage));
            }
            
            // If validation fails, return to view
            return View(user);
        }

        private List<User> GetSampleUsers()
        {
            return new List<User>
            {
                new User { UserId = 1, FirstName = "John", LastName = "Smith", Email = "john.smith@routex.com", Role = "Admin", Status = "Active" },
                new User { UserId = 2, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@routex.com", Role = "Operations", Status = "Active" },
                new User { UserId = 3, FirstName = "Michael", LastName = "Brown", Email = "michael.brown@routex.com", Role = "Finance", Status = "Inactive" },
                new User { UserId = 4, FirstName = "Emily", LastName = "Davis", Email = "emily.davis@routex.com", Role = "Operations", Status = "Active" },
                new User { UserId = 5, FirstName = "Robert", LastName = "Wilson", Email = "robert.wilson@routex.com", Role = "Admin", Status = "Active" }
            };
        }

        private List<Role> GetActiveRoles()
        {
            // Fixed roles as specified
            return new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Super Admin", Status = "Active", Description = "Full system administration with all privileges" },
                new Role { RoleId = 2, RoleName = "Admin", Status = "Active", Description = "System administration with user management privileges" },
                new Role { RoleId = 3, RoleName = "Operations", Status = "Active", Description = "Vehicle operations and dispatch management" },
                new Role { RoleId = 4, RoleName = "Finance", Status = "Active", Description = "Financial reporting and expense management" }
            };
        }

        // GET: Users/EditUser/5
        public IActionResult EditUser(int id)
        {
            ViewData["Title"] = "Edit User";
            
            // Get all users (sample data)
            var allUsers = GetSampleUsers();
            
            // Find specific user by ID
            var user = allUsers.FirstOrDefault(u => u.UserId == id);
            
            // If not found, create a default user
            if (user == null)
            {
                user = new User
                {
                    UserId = id,
                    FirstName = "User not found",
                    LastName = "",
                    Email = "",
                    Role = "Admin",
                    Status = "Active"
                };
            }
            
            // Get active roles for dropdown
            var activeRoles = GetActiveRoles();
            ViewBag.ActiveRoles = activeRoles;
            
            return View(user);
        }

        // POST: Users/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would update the user in the database here
                // For now, we'll just redirect back to UsersPage with success message
                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(UsersPage));
            }
            
            // If validation fails, return to view
            return View(user);
        }

        // GET: Users/ViewUser/5
        public IActionResult ViewUser(int id)
        {
            ViewData["Title"] = "View User";
            
            // Get all users (sample data)
            var allUsers = GetSampleUsers();
            
            // Find specific user by ID
            var user = allUsers.FirstOrDefault(u => u.UserId == id);
            
            // If not found, create a default user
            if (user == null)
            {
                user = new User
                {
                    UserId = id,
                    FirstName = "User not found",
                    LastName = "",
                    Email = "",
                    Role = "Admin",
                    Status = "Active"
                };
            }
            
            return View(user);
        }
    }
}
