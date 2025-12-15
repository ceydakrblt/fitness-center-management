using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;

namespace WebProgramlamaOdev.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AiRequestsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AiRequestsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AiRequestsApi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AiExerciseRequests
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.Height,
                    x.Weight,
                    x.Goal,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/AiRequestsApi/byUser/{userId}
        [HttpGet("byUser/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var data = await _context.AiExerciseRequests
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(data);
        }
    }
}
