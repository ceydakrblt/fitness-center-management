using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;

namespace WebProgramlamaOdev.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrainerAvailabilityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerAvailabilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/TrainerAvailability
        public async Task<IActionResult> Index()
        {
            var availabilities = _context.TrainerAvailabilities.Include(t => t.Trainer);
            return View(await availabilities.ToListAsync());
        }

        // GET: Admin/TrainerAvailability/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainerAvailability = await _context.TrainerAvailabilities
                .Include(t => t.Trainer)
                .FirstOrDefaultAsync(m => m.AvailabilityId == id);
            if (trainerAvailability == null)
            {
                return NotFound();
            }

            return View(trainerAvailability);
        }

        // GET: Admin/TrainerAvailability/Create
        public IActionResult Create()
        {
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName");
            return View();
        }

        // POST: Admin/TrainerAvailability/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AvailabilityId,TrainerId,DayOfWeek,StartTime,EndTime,IsActive")] TrainerAvailability trainerAvailability)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainerAvailability);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", trainerAvailability.TrainerId);
            return View(trainerAvailability);
        }

        // GET: Admin/TrainerAvailability/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainerAvailability = await _context.TrainerAvailabilities.FindAsync(id);
            if (trainerAvailability == null)
            {
                return NotFound();
            }
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", trainerAvailability.TrainerId);
            return View(trainerAvailability);
        }

        // POST: Admin/TrainerAvailability/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AvailabilityId,TrainerId,DayOfWeek,StartTime,EndTime,IsActive")] TrainerAvailability trainerAvailability)
        {
            if (id != trainerAvailability.AvailabilityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainerAvailability);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerAvailabilityExists(trainerAvailability.AvailabilityId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", trainerAvailability.TrainerId);
            return View(trainerAvailability);
        }

        // GET: Admin/TrainerAvailability/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainerAvailability = await _context.TrainerAvailabilities
                .Include(t => t.Trainer)
                .FirstOrDefaultAsync(m => m.AvailabilityId == id);
            if (trainerAvailability == null)
            {
                return NotFound();
            }

            return View(trainerAvailability);
        }

        // POST: Admin/TrainerAvailability/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainerAvailability = await _context.TrainerAvailabilities.FindAsync(id);
            if (trainerAvailability != null)
            {
                _context.TrainerAvailabilities.Remove(trainerAvailability);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerAvailabilityExists(int id)
        {
            return _context.TrainerAvailabilities.Any(e => e.AvailabilityId == id);
        }
    }
}