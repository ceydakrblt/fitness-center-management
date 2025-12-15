using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;
using WebProgramlamaOdev.Services;

namespace WebProgramlamaOdev.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly OpenAiService _openAi;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AiController(
            OpenAiService openAi,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _openAi = openAi;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(int height, int weight, string goal)
        {
            var result = await _openAi.GetExercisePlan(height, weight, goal);

            var user = await _userManager.GetUserAsync(User);

            var record = new AiExerciseRequest
            {
                UserId = user.Id,
                Height = height,
                Weight = weight,
                Goal = goal,
                AiResponse = result,
                CreatedAt = DateTime.Now
            };

            _context.AiExerciseRequests.Add(record);
            await _context.SaveChangesAsync();

            ViewBag.Result = result;
            return View();
        }
    }
}
