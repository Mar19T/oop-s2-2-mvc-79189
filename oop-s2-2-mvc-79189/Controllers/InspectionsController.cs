using Inspections.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_79189.Data;

namespace oop_s2_2_mvc_79189.Controllers
{
    [Authorize]
    public class InspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InspectionsController> _logger;

        public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Inspector,Viewer")]
        public async Task<IActionResult> Index()
        {
            var inspections = await _context.Inspections
                .Include(i => i.Premises)
                .ToListAsync();
            return View(inspections);
        }

        [Authorize(Roles = "Admin,Inspector,Viewer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null) return NotFound();
            return View(inspection);
        }

        // GET: Inspections/Create
        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            // ✅ Shows "Name — Town" so duplicates are distinguishable
            ViewData["PremisesId"] = new SelectList(
                _context.Premises.Select(p => new { p.Id, Display = p.Name + " — " + p.Address }),
                "Id", "Display");
            return View();
        }

        // POST: Inspections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            // ✅ Remove navigation property from validation
            ModelState.Remove("Premises");

            // ✅ Score range validation
            if (inspection.Score < 0 || inspection.Score > 100)
            {
                ModelState.AddModelError("Score", "Score must be between 0 and 100.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(inspection);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Inspection created for PremisesId {PremisesId}, Score {Score}, Outcome {Outcome}",
                    inspection.PremisesId, inspection.Score, inspection.Outcome);

                return RedirectToAction(nameof(Index));
            }

            // ✅ Log validation errors to help diagnose issues
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Inspection create validation error: {Error}", error.ErrorMessage);
            }

            ViewData["PremisesId"] = new SelectList(
                _context.Premises.Select(p => new { p.Id, Display = p.Name + " — " + p.Address }),
                "Id", "Display", inspection.PremisesId);
            return View(inspection);
        }

        // GET: Inspections/Edit
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inspection = await _context.Inspections
                .Include(i => i.Premises) // ✅ load premises for display
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inspection == null) return NotFound();

            // ✅ Pass display name to view — premises is locked on edit
            ViewBag.PremisesDisplay = inspection.Premises?.Name + " — " + inspection.Premises?.Address;

            return View(inspection);
        }

        // POST: Inspections/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            if (id != inspection.Id) return NotFound();

            // ✅ Remove navigation property from validation
            ModelState.Remove("Premises");

            // ✅ Score range validation
            if (inspection.Score < 0 || inspection.Score > 100)
            {
                ModelState.AddModelError("Score", "Score must be between 0 and 100.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspection);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Inspection {InspectionId} updated, Score {Score}, Outcome {Outcome}",
                        inspection.Id, inspection.Score, inspection.Outcome);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!InspectionExists(inspection.Id))
                        return NotFound();

                    _logger.LogError(ex,
                        "Concurrency error updating Inspection {InspectionId}",
                        inspection.Id);
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // ✅ Reload display name if validation fails
            var premises = await _context.Premises.FindAsync(inspection.PremisesId);
            ViewBag.PremisesDisplay = premises?.Name + " — " + premises?.Address;

            return View(inspection);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null) return NotFound();
            return View(inspection);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
            {
                _context.Inspections.Remove(inspection);
                _logger.LogInformation("Inspection {InspectionId} deleted", id);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.Id == id);
        }
    }
}