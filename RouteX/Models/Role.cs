namespace RouteX.Models
{
    // Simple Role class for user dropdown - not stored in database
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
