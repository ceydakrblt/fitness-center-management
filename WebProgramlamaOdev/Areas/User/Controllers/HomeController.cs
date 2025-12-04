using Microsoft.AspNetCore.Mvc;

namespace WebProgramlamaOdev.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

}
