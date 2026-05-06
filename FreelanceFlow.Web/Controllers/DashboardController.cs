using FreelanceFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FreelanceFlow.Web.Extensions;
using FreelanceFlow.Web.Models;

namespace FreelanceFlow.Web.Controllers;

public class DashboardController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var history = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
        var stats = new DashboardStats
        {
            TotalDocuments = history.Count,
            ThisMonthCount = history.Count(x => x.CreatedAt.Month == DateTime.UtcNow.Month && x.CreatedAt.Year == DateTime.UtcNow.Year),
            ModuleUsage = history.GroupBy(x => x.Module).ToDictionary(g => g.Key, g => g.Count()),
            Currency = "TL"
        };

        return View(stats);
    }
}
