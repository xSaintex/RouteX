using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RouteX.Models;
using RouteX.Data;
using System.Security.Claims;

namespace RouteX.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(ClaimsPrincipal user, string action);
        Task LogActionAsync(string userId, string action);
        Task<List<AuditLog>> GetAuditLogsAsync();
        Task<(List<AuditLog> Logs, int TotalCount)> GetAuditLogsPagedAsync(int page = 1, int pageSize = 15);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string FormatActionAsSentence(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return action;

            // If it's already a sentence, return as-is
            if (!action.Contains(":") && (action.Contains(" ") || action.EndsWith(".")))
                return action;

            // Handle colon-separated format: Action:Entity:Details
            var parts = action.Split(':');
            
            if (parts.Length == 0)
                return action;

            var actionType = parts[0];
            var entity = parts.Length > 1 ? parts[1] : "";
            var details = parts.Length > 2 ? parts[2] : "";

            // Handle special cases with additional details
            if (parts.Length > 3)
            {
                // Handle cases like "Approve:Vehicle:123:AddedBy:user@email.com"
                if (actionType == "Approve" && parts.Length > 3 && parts[2] == "AddedBy")
                {
                    return $"Approved {entity} record {details} that was added by {parts[3]}";
                }
                if (actionType == "Reject" && parts.Length > 3 && parts[2] == "AddedBy")
                {
                    return $"Rejected {entity} record {details} that was added by {parts[3]}";
                }
                if (actionType == "Archive" && parts.Length > 3 && parts[2] == "Status")
                {
                    return $"Archived {entity} record {details} with status {parts[3]}";
                }
            }

            return actionType switch
            {
                "Create" => $"Created a new {entity} record{(string.IsNullOrEmpty(details) ? "" : $" with ID {details}")}",
                "Update" => $"Updated {entity} record{(string.IsNullOrEmpty(details) ? "" : $" with ID {details}")}",
                "Delete" => $"Deleted {entity} record{(string.IsNullOrEmpty(details) ? "" : $" with ID {details}")}",
                "Archive" => $"Archived {entity} record{(string.IsNullOrEmpty(details) ? "" : $" with ID {details}")}",
                "Approve" => $"Approved {entity} record{(string.IsNullOrEmpty(details) ? "" : $" with ID {details}")}",
                "Reject" => $"Rejected {entity} record{(string.IsNullOrEmpty(details) ? "" : $" with ID {details}")}",
                "Export" => $"Exported {entity} report in {details} format",
                _ => action // Return original if no pattern matches
            };
        }

        public async Task LogActionAsync(ClaimsPrincipal user, string action)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
                
                // Use email as UserId for display purposes
                await LogActionAsync(userEmail, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action for user");
            }
        }

        public async Task LogActionAsync(string userId, string action)
        {
            try
            {
                var formattedAction = FormatActionAsSentence(action);
                
                var auditLog = new AuditLog
                {
                    UserId = userId, // This will store the email for display
                    Action = formattedAction,
                    ActionDate = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action: {Action} for user {UserId}", action, userId);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync()
        {
            try
            {
                return await _context.AuditLogs
                    .AsNoTracking()
                    .Where(a => a.UserId != "SYSTEM")
                    .OrderByDescending(a => a.ActionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return new List<AuditLog>();
            }
        }

        public async Task<(List<AuditLog> Logs, int TotalCount)> GetAuditLogsPagedAsync(int page = 1, int pageSize = 15)
        {
            try
            {
                var query = _context.AuditLogs
                    .AsNoTracking()
                    .Where(a => a.UserId != "SYSTEM")
                    .OrderByDescending(a => a.ActionDate);

                var totalCount = await query.CountAsync();
                var logs = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (logs, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged audit logs");
                return (new List<AuditLog>(), 0);
            }
        }
    }
}
