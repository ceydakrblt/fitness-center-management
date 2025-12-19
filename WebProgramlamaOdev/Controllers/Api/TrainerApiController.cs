using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;

namespace WebProgramlamaOdev.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1️⃣ TUM ANTRENORLERI LISTELE
        // GET: api/TrainersApi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var trainers = await _context.Trainers
                .Select(t => new
                {
                    t.TrainerId,
                    t.FullName,
                    t.Specialization
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // 2️⃣ BELIRLI BIR TARIHTE UYGUN ANTRENORLER
        // GET: api/TrainersApi/available?date=2025-01-20
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            var trainers = await _context.TrainerAvailabilities
                .Where(a =>
                    a.DayOfWeek == dayOfWeek &&
                    a.IsActive
                )
                .Select(a => new
                {
                    a.Trainer.TrainerId,
                    a.Trainer.FullName,
                    a.Trainer.Specialization,
                    a.StartTime,
                    a.EndTime
                })
                .Distinct()
                .ToListAsync();

            return Ok(trainers);
        }
    }
}
