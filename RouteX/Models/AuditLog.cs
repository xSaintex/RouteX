using System;
using System.ComponentModel.DataAnnotations;

namespace RouteX.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }
        public string? UserId { get; set; }
        public string? Action { get; set; }
        public string? RawAction { get; set; }
        public DateTime ActionDate { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }
}
