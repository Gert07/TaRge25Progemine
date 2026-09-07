using Microsoft.AspNetCore.Mvc;

namespace TaRge25Shop.Controllers
{
    public class SpaceshipController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
