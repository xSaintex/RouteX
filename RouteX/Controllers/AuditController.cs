using Microsoft.AspNetCore.Mvc;
using RouteX.Models;
using RouteX.Services;
using System.Linq;

namespace RouteX.Controllers
{
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<IActionResult> AuditPage(int page = 1)
        {
            try
            {
                ViewData["Title"] = "Audit Logs";
                
                const int pageSize = 15;
                var (auditLogs, totalCount) = await _auditService.GetAuditLogsPagedAsync(page, pageSize);
                
                // Calculate pagination info
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                // Pass pagination data to ViewBag
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;
                ViewBag.HasPreviousPage = page > 1;
                ViewBag.HasNextPage = page < totalPages;
                
                // Log that someone viewed the audit page
                await _auditService.LogActionAsync(User, "Viewed Audit Page");
                
                return View(auditLogs);
            }
            catch (Exception)
            {
                // If there's an error, return empty list with pagination info
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = 15;
                ViewBag.HasPreviousPage = false;
                ViewBag.HasNextPage = false;
                
                return View(new List<AuditLog>());
            }
        }
    }
}
