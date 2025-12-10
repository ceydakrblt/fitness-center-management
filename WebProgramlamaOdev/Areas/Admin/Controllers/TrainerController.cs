using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WebProgramlamaOdev.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Trainer (LIST)
        public async Task<IActionResult> Index()
        {
            // Gym ilişkisini yüklüyoruz
            var trainers = await _context.Trainers.Include(t => t.Gym).ToListAsync();
            return View(trainers);
        }

        // GET: Admin/Trainer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.Gym) // Gym bilgisini dahil et
                .FirstOrDefaultAsync(m => m.TrainerId == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // GET: Admin/Trainer/Create
        public IActionResult Create()
        {
            // Salonları dropdown listesi için yüklüyoruz
            ViewBag.GymId = new SelectList(_context.Gyms, "GymId", "Name");
            return View();
        }

        // POST: Admin/Trainer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TrainerId,FullName,Email,PhoneNumber,Specialization,Biography,PhotoUrl,GymId")] Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata durumunda salon listesini tekrar yükle
            ViewBag.GymId = new SelectList(_context.Gyms, "GymId", "Name", trainer.GymId);
            return View(trainer);
        }

        // GET: Admin/Trainer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) return NotFound();

            // Salonları, SelectList ile View'a gönder (mevcut salon seçili gelmeli)
            ViewBag.GymId = new SelectList(_context.Gyms, "GymId", "Name", trainer.GymId);
            return View(trainer);
        }

        // POST: Admin/Trainer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("TrainerId,FullName,Email,PhoneNumber,Specialization,Biography,PhotoUrl,GymId")] Trainer trainer)
        {
            if (id != trainer.TrainerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Trainers.Any(e => e.TrainerId == id))
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

            // Hata durumunda salon listesini tekrar yükle
            ViewBag.GymId = new SelectList(_context.Gyms, "GymId", "Name", trainer.GymId);
            return View(trainer);
        }

        // GET: Admin/Trainer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .FirstOrDefaultAsync(m => m.TrainerId == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // POST: Admin/Trainer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);

            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}