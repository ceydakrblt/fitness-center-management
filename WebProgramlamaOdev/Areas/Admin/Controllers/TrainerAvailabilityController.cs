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

        // GET: Admin/TrainerAvailability (LIST)
        public async Task<IActionResult> Index()
        {
            // Eğitmen adlarını göstermek için Trainer ilişkisini yüklüyoruz
            var availabilities = await _context.TrainerAvailabilities
                .Include(ta => ta.Trainer)
                .ToListAsync();

            return View(availabilities);
        }

        // GET: Admin/TrainerAvailability/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var availability = await _context.TrainerAvailabilities
                .Include(ta => ta.Trainer)
                .FirstOrDefaultAsync(m => m.AvailabilityId == id);

            if (availability == null) return NotFound();

            return View(availability);
        }

        // GET: Admin/TrainerAvailability/Create
        public IActionResult Create()
        {
            ViewBag.TrainerId = new SelectList(_context.Trainers, "TrainerId", "FirstName");
            return View();
        }

        // POST: Admin/TrainerAvailability/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AvailabilityId,TrainerId,DayOfWeek,StartTime,EndTime,IsActive")] TrainerAvailability availability)
        {
            if (ModelState.IsValid)
            {
                if (availability.StartTime >= availability.EndTime)
                {
                    ModelState.AddModelError("EndTime", "Bitiş Saati, Başlangıç Saatinden sonra olmalıdır.");
                }
                else if (IsConflict(availability))
                {
                    ModelState.AddModelError("", "Bu eğitmenin, aynı gün içinde bu saat aralığıyla çakışan bir müsaitlik kaydı zaten mevcut.");
                }
                else
                {
                    _context.Add(availability);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.TrainerId = new SelectList(_context.Trainers, "TrainerId", "FirstName", availability.TrainerId);
            return View(availability);
        }

        // GET: Admin/TrainerAvailability/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var availability = await _context.TrainerAvailabilities.FindAsync(id);
            if (availability == null) return NotFound();

            ViewBag.TrainerId = new SelectList(_context.Trainers, "TrainerId", "FirstName", availability.TrainerId);
            return View(availability);
        }

        // POST: Admin/TrainerAvailability/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("AvailabilityId,TrainerId,DayOfWeek,StartTime,EndTime,IsActive")] TrainerAvailability availability)
        {
            if (id != availability.AvailabilityId) return NotFound();

            if (ModelState.IsValid)
            {
                if (availability.StartTime >= availability.EndTime)
                {
                    ModelState.AddModelError("EndTime", "Bitiş Saati, Başlangıç Saatinden sonra olmalıdır.");
                }
                else if (IsConflict(availability, id))
                {
                    ModelState.AddModelError("", "Bu eğitmenin, aynı gün içinde bu saat aralığıyla çakışan bir müsaitlik kaydı zaten mevcut.");
                }
                else
                {
                    try
                    {
                        _context.Update(availability);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.TrainerAvailabilities.Any(e => e.AvailabilityId == id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            ViewBag.TrainerId = new SelectList(_context.Trainers, "TrainerId", "FirstName", availability.TrainerId);
            return View(availability);
        }

        // GET: Admin/TrainerAvailability/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var availability = await _context.TrainerAvailabilities
                .Include(ta => ta.Trainer)
                .FirstOrDefaultAsync(m => m.AvailabilityId == id);

            if (availability == null) return NotFound();

            return View(availability);
        }

        // POST: Admin/TrainerAvailability/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var availability = await _context.TrainerAvailabilities.FindAsync(id);

            if (availability != null)
            {
                _context.TrainerAvailabilities.Remove(availability);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Yardımcı Metot: Çakışma Kontrolü (Randevu Sistemi için kritik)
        private bool IsConflict(TrainerAvailability newAvailability, int? currentId = null)
        {
            // Kendisi hariç tüm kayıtları sorgula
            var query = _context.TrainerAvailabilities.AsQueryable();
            if (currentId.HasValue)
            {
                query = query.Where(ta => ta.AvailabilityId != currentId.Value);
            }

            return query
                .Any(ta =>
                    ta.TrainerId == newAvailability.TrainerId &&
                    ta.DayOfWeek == newAvailability.DayOfWeek &&
                    ta.IsActive == true &&
                    // Çakışma Kontrolü Mantığı: Yeni aralık, mevcut aralığın ne zaman başladığı ve bittiği
                    !(ta.EndTime <= newAvailability.StartTime || ta.StartTime >= newAvailability.EndTime));
        }
    }
}