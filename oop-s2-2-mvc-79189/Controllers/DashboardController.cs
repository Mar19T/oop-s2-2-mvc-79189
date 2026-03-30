using Inspections.Domain;
using Microsoft.AspNetCore.Mvc;
using oop_s2_2_mvc_79189.Data;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var totalInspections = _context.Inspections.Count();

        var failedInspections = _context.Inspections
            .Count(i => i.Score < 70);

        var overdueFollowUps = _context.FollowUps
            .Count(f => f.DueDate < DateTime.Now && f.Status != FollowUpStatus.Open);

        ViewBag.Total = totalInspections;
        ViewBag.Failed = failedInspections;
        ViewBag.Overdue = overdueFollowUps;

        // LOGGING
        _logger.LogInformation("Dashboard viewed");

        // Log warnings for failed inspections and overdue follow-ups
        if (overdueFollowUps > 0)
        {
            _logger.LogWarning("There are overdue follow-ups: {Count}", overdueFollowUps);
        }

        return View();
    }
}