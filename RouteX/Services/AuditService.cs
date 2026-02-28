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
                var auditLog = new AuditLog
                {
                    UserId = userId, // This will store the email for display
                    Action = action,
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
