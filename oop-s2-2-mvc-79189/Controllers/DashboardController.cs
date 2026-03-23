using Microsoft.AspNetCore.Mvc;
using oop_s2_2_mvc_79189.Data;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var totalInspections = _context.Inspections.Count();

        var failedInspections = _context.Inspections
            .Count(i => i.Score < 70);

        var overdueFollowUps = _context.FollowUps
            .Count(f => f.DueDate < DateTime.Now && f.Status != "Closed");

        ViewBag.Total = totalInspections;
        ViewBag.Failed = failedInspections;
        ViewBag.Overdue = overdueFollowUps;

        return View();
    }
}