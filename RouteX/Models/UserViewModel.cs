using System.ComponentModel.DataAnnotations;

namespace RouteX.Models
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [StringLength(12, ErrorMessage = "Password cannot exceed 12 characters")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [StringLength(12, ErrorMessage = "Password cannot exceed 12 characters")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;

        // Branch assignment - null for SuperAdmin
        public int? BranchId { get; set; }
    }

    public class CreateUserViewModel : UserViewModel
    {
        // Additional properties specific to user creation
        public bool IsEditMode { get; set; } = false;
    }

    public class EditUserViewModel : UserViewModel
    {
        // For edit mode, password is optional
        public new string Password { get; set; } = string.Empty;
        public new string ConfirmPassword { get; set; } = string.Empty;
        
        // Flag to indicate if password should be updated
        public bool UpdatePassword { get; set; } = false;
    }
}
