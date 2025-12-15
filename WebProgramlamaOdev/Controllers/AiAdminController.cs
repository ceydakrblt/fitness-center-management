using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;

namespace WebProgramlamaOdev.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AiAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AiAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.AiExerciseRequests
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.AiExerciseRequests.FindAsync(id);

            if (record != null)
            {
                _context.AiExerciseRequests.Remove(record);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }


}
