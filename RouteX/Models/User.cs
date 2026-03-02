using System.ComponentModel.DataAnnotations.Schema;

namespace RouteX.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        // Branch relationship - Employee belongs to one branch (nullable for SuperAdmin who can access all)
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Archived
    }
}
