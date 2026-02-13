using Microsoft.AspNetCore.Mvc;
using RouteX.Models;
using System.Linq;

namespace RouteX.Controllers
{
    public class AuditController : Controller
    {
        public IActionResult AuditPage()
        {
            // Sample audit data - in real app, this would come from database
            var auditLogs = new List<AuditLog>
            {
                new AuditLog { UserId = "admin@routex.com", Action = "Login", ActionDate = DateTime.Now.AddMinutes(-5) },
                new AuditLog { UserId = "john.doe@routex.com", Action = "Created Vehicle", ActionDate = DateTime.Now.AddMinutes(-15) },
                new AuditLog { UserId = "admin@routex.com", Action = "Updated Route", ActionDate = DateTime.Now.AddMinutes(-30) },
                new AuditLog { UserId = "jane.smith@routex.com", Action = "Deleted Maintenance Record", ActionDate = DateTime.Now.AddMinutes(-45) },
                new AuditLog { UserId = "mike.wilson@routex.com", Action = "Generated Report", ActionDate = DateTime.Now.AddHours(-1) },
                new AuditLog { UserId = "admin@routex.com", Action = "Modified User", ActionDate = DateTime.Now.AddHours(-2) },
                new AuditLog { UserId = "sarah.jones@routex.com", Action = "Added Fuel Record", ActionDate = DateTime.Now.AddHours(-3) },
                new AuditLog { UserId = "john.doe@routex.com", Action = "Exported Data", ActionDate = DateTime.Now.AddHours(-4) },
                new AuditLog { UserId = "admin@routex.com", Action = "System Backup", ActionDate = DateTime.Now.AddHours(-5) },
                new AuditLog { UserId = "david.brown@routex.com", Action = "Updated Vehicle Status", ActionDate = DateTime.Now.AddHours(-6) },
                new AuditLog { UserId = "jane.smith@routex.com", Action = "Created Route", ActionDate = DateTime.Now.AddDays(-1) },
                new AuditLog { UserId = "mike.wilson@routex.com", Action = "Deleted Vehicle", ActionDate = DateTime.Now.AddDays(-1) },
                new AuditLog { UserId = "sarah.jones@routex.com", Action = "Modified Maintenance", ActionDate = DateTime.Now.AddDays(-2) },
                new AuditLog { UserId = "admin@routex.com", Action = "User Login", ActionDate = DateTime.Now.AddDays(-2) },
                new AuditLog { UserId = "david.brown@routex.com", Action = "Report Generation", ActionDate = DateTime.Now.AddDays(-3) }
            };

            return View(auditLogs);
        }
    }
}
