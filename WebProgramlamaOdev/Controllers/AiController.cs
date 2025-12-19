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
        public async Task<IActionResult> Index(AiExerciseRequest model)
        {
            // SERVER-SIDE VALIDATION
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _openAi.GetExercisePlan(
                model.Height,
                model.Weight,
                model.Goal
            );

            var user = await _userManager.GetUserAsync(User);

            model.UserId = user.Id;
            model.AiResponse = result;
            model.CreatedAt = DateTime.Now;

            _context.AiExerciseRequests.Add(model);
            await _context.SaveChangesAsync();

            ViewBag.Result = result;

            return View();
        }
    }
}
