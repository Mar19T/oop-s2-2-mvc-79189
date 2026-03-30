using Inspections.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_79189.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: FollowUps
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FollowUps.Include(f => f.Inspection);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FollowUps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (followUp == null)
            {
                return NotFound();
            }

            return View(followUp);
        }

        // GET: FollowUps/Create
        public IActionResult Create()
        {
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Notes");
            return View();
        }

        // POST: FollowUps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (ModelState.IsValid)
            {
                // ✅ NEW — business rule: due date cannot be before inspection date
                var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
                if (inspection != null && followUp.DueDate < inspection.InspectionDate)
                {
                    // ✅ NEW — Warning log for business rule violation
                    _logger.LogWarning(
                        "FollowUp DueDate {DueDate} is before InspectionDate {InspectionDate} for InspectionId {InspectionId}",
                        followUp.DueDate, inspection.InspectionDate, followUp.InspectionId);

                    ModelState.AddModelError("DueDate", "Due date cannot be before the inspection date.");
                    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Notes", followUp.InspectionId);
                    return View(followUp);
                }

                _context.Add(followUp);
                await _context.SaveChangesAsync();

                // ✅ NEW — Information log for successful creation
                _logger.LogInformation(
                    "FollowUp created for InspectionId {InspectionId}, DueDate {DueDate}, Status {Status}",
                    followUp.InspectionId, followUp.DueDate, followUp.Status);

                return RedirectToAction(nameof(Index));
            }

            // ✅ NEW — Warning log for validation failure
            _logger.LogWarning(
                "FollowUp creation failed validation for InspectionId {InspectionId}",
                followUp.InspectionId);

            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Notes", followUp.InspectionId);
            return View(followUp);
        }


        // GET: FollowUps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null)
            {
                return NotFound();
            }
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Notes", followUp.InspectionId);
            return View(followUp);
        }

        // POST: FollowUps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (ModelState.IsValid)
            {
                // ✅ NEW — business rule: cannot close without a ClosedDate
                if (followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null)
                {
                    _logger.LogWarning(
                        "Attempt to close FollowUp {FollowUpId} without a ClosedDate",
                        followUp.Id);

                    ModelState.AddModelError("ClosedDate", "A closed date is required when closing a follow-up.");
                    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Notes", followUp.InspectionId);
                    return View(followUp);
                }

                try
                {
                    _context.Update(followUp);
                    await _context.SaveChangesAsync();

                    // ✅ NEW — Information log for successful edit
                    _logger.LogInformation(
                        "FollowUp {FollowUpId} updated, Status {Status}, ClosedDate {ClosedDate}",
                        followUp.Id, followUp.Status, followUp.ClosedDate);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!FollowUpExists(followUp.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        // ✅ NEW — Error log for unexpected exception
                        _logger.LogError(ex,
                            "Concurrency error updating FollowUp {FollowUpId}",
                            followUp.Id);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Notes", followUp.InspectionId);
            return View(followUp);
        }

        // GET: FollowUps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (followUp == null)
            {
                return NotFound();
            }

            return View(followUp);
        }

        // POST: FollowUps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                // ✅ NEW — Information log for deletion
                _logger.LogInformation(
                    "FollowUp {FollowUpId} deleted", id);

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
