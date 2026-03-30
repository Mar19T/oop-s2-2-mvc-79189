using Inspections.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_79189.Data;

namespace oop_s2_2_mvc_79189.Controllers
{
    [Authorize]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpsController> _logger;

        public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Inspector,Viewer")]
        public async Task<IActionResult> Index()
        {
            var followUps = await _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                .ToListAsync();
            return View(followUps);
        }

        [Authorize(Roles = "Admin,Inspector,Viewer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();
            return View(followUp);
        }

        // GET: FollowUps/Create
        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            // ✅ Show "Premises Name — Date" so inspector knows which inspection they're picking
            ViewData["InspectionId"] = new SelectList(
                _context.Inspections
                    .Include(i => i.Premises)
                    .Select(i => new
                    {
                        i.Id,
                        Display = i.Premises.Name  + " - " +i.Premises.Address + " — " + i.InspectionDate.ToString("dd MMM yyyy")
                    }),
                "Id", "Display");

            // ✅ Pre-fill DueDate to 2 weeks from today
            ViewBag.DefaultDueDate = DateTime.Today.AddDays(14).ToString("yyyy-MM-ddTHH:mm");

            return View();
        }

        // POST: FollowUps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status")] FollowUp followUp)
        {
            // ✅ Remove navigation property from validation
            ModelState.Remove("Inspection");

            // ✅ Business rule: due date cannot be before inspection date
            var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
            if (inspection != null && followUp.DueDate < inspection.InspectionDate)
            {
                _logger.LogWarning(
                    "FollowUp DueDate {DueDate} is before InspectionDate {InspectionDate} for InspectionId {InspectionId}",
                    followUp.DueDate, inspection.InspectionDate, followUp.InspectionId);

                ModelState.AddModelError("DueDate", "Due date cannot be before the inspection date.");
            }

            if (ModelState.IsValid)
            {
                // ✅ ClosedDate is always null on creation
                followUp.ClosedDate = null;

                _context.Add(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "FollowUp created for InspectionId {InspectionId}, DueDate {DueDate}, Status {Status}",
                    followUp.InspectionId, followUp.DueDate, followUp.Status);

                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning(
                "FollowUp creation failed validation for InspectionId {InspectionId}",
                followUp.InspectionId);

            // ✅ Reload dropdown and default date on validation failure
            ViewData["InspectionId"] = new SelectList(
                _context.Inspections
                    .Include(i => i.Premises)
                    .Select(i => new
                    {
                        i.Id,
                        Display = i.Premises.Name  + " - " + i.Premises.Address + " — " + i.InspectionDate.ToString("dd MMM yyyy")
                    }),
                "Id", "Display", followUp.InspectionId);

            ViewBag.DefaultDueDate = followUp.DueDate.ToString("yyyy-MM-ddTHH:mm");
            return View(followUp);
        }

        // GET: FollowUps/Edit
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (followUp == null) return NotFound();

            // ✅ Show locked inspection display
            ViewBag.InspectionDisplay = followUp.Inspection?.Premises?.Name
                + " — "
                + followUp.Inspection?.InspectionDate.ToString("dd MMM yyyy");

            return View(followUp);
        }

        // POST: FollowUps/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            // ✅ Remove navigation property from validation
            ModelState.Remove("Inspection");

            // ✅ Business rule: cannot close without ClosedDate
            if (followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null)
            {
                _logger.LogWarning(
                    "Attempt to close FollowUp {FollowUpId} without a ClosedDate",
                    followUp.Id);

                ModelState.AddModelError("ClosedDate", "A closed date is required when closing a follow-up.");
            }
            // ✅ ADD THIS BELOW — ClosedDate cannot be in the future
            if (followUp.ClosedDate.HasValue && followUp.ClosedDate.Value > DateTime.Today)
            {
                _logger.LogWarning(
                    "Attempt to set future ClosedDate {ClosedDate} for FollowUp {FollowUpId}",
                    followUp.ClosedDate, followUp.Id);

                ModelState.AddModelError("ClosedDate", "Closed date cannot be in the future.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(followUp);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "FollowUp {FollowUpId} updated, Status {Status}, ClosedDate {ClosedDate}",
                        followUp.Id, followUp.Status, followUp.ClosedDate);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!FollowUpExists(followUp.Id))
                        return NotFound();

                    _logger.LogError(ex,
                        "Concurrency error updating FollowUp {FollowUpId}",
                        followUp.Id);
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // ✅ Reload inspection display on validation failure
            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(i => i.Id == followUp.InspectionId);

            ViewBag.InspectionDisplay = inspection?.Premises?.Name
                + " — "
                + inspection?.InspectionDate.ToString("dd MMM yyyy");

            return View(followUp);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();
            return View(followUp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                _logger.LogInformation("FollowUp {FollowUpId} deleted", id);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }
    }
}