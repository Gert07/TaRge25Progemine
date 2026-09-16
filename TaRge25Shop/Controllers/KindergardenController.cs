using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Data;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Models.Kindergarden

namespace TaRge25Shop.Controllers
{
    public class KindergardenController : Controller
    {
        private readonly IKindergardenServices _kindergardenServices;
        private readonly TaRge25ShopContext _context;

        public KindergardenController
            (
                IKindergardenServices kindergardenServices,
                TaRge25ShopContext context
            )
        {
            _kindergardenServices = kindergardenServices;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
