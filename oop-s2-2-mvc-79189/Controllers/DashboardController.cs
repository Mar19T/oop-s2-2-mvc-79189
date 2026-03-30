using Inspections.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_79189.Data;
using oop_s2_2_mvc_79189.Models;

namespace oop_s2_2_mvc_79189.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? town, string? riskRating)
        {
            _logger.LogInformation(
                "Dashboard accessed with filters Town={Town}, RiskRating={RiskRating}",
                town, riskRating);

            var now = DateTime.Today;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // ── Aggregation queries ────────────────────────────
            var inspectionsThisMonth = await _context.Inspections
                .Where(i => i.InspectionDate >= startOfMonth)
                .CountAsync();

            var failedThisMonth = await _context.Inspections
                .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == Outcome.Fail)
                .CountAsync();

            var overdueFollowUps = await _context.FollowUps
                .Where(f => f.DueDate < now && f.Status == FollowUpStatus.Open)
                .CountAsync();

            // ── Filtered inspections table ─────────────────────
            var query = _context.Inspections
                .Include(i => i.Premises)
                .AsQueryable();

            if (!string.IsNullOrEmpty(town))
                query = query.Where(i => i.Premises.Town == town);

            if (!string.IsNullOrEmpty(riskRating) && Enum.TryParse<RiskRating>(riskRating, out var rr))
                query = query.Where(i => i.Premises.RiskRating == rr);

            var inspections = await query
                .OrderByDescending(i => i.InspectionDate)
                .Select(i => new InspectionRow
                {
                    PremisesName = i.Premises.Name,
                    Town = i.Premises.Town,
                    RiskRating = i.Premises.RiskRating.ToString(),
                    InspectionDate = i.InspectionDate,
                    Score = i.Score,
                    Outcome = i.Outcome.ToString()
                })
                .ToListAsync();

            // ── Towns dropdown ─────────────────────────────────
            var towns = await _context.Premises
                .Select(p => p.Town)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                InspectionsThisMonth = inspectionsThisMonth,
                FailedInspectionsThisMonth = failedThisMonth,
                OverdueFollowUps = overdueFollowUps,
                SelectedTown = town,
                SelectedRiskRating = riskRating,
                Towns = towns,
                Inspections = inspections
            };

            return View(vm);
        }
    }
}