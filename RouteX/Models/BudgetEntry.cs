using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RouteX.Models
{
    public class BudgetEntry
    {
        [Key]
        public int Id { get; set; }
        public string Month { get; set; } = string.Empty;
        [Column("BudgetAmount")]
        public decimal BudgetAmount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Branch relationship - BudgetEntry belongs to one branch
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
