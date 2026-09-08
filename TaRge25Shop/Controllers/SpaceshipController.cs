using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Models.Spaceship;
using TaRge25Shop.Core.ServiceInterface;

namespace TaRge25Shop.Controllers
{
    public class SpaceshipController : Controller
    {
        private readonly ISpaceshipServices _spaceshipServices;

        public SpaceshipController(ISpaceshipServices spaceshipServices)
        {
            _spaceshipServices = spaceshipServices;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(SpaceshipCreateViewModel vm)
        {
            var dto = new SpaceshipDto
            {
                Name = vm.Name,
                ShipType = vm.ShipType,
                Crew = vm.Crew,
                EnginePower = vm.EnginePower,
            };

            //Nüüd kutsume teenuse välja, et luua uus kosmoselaev
            //See on asünkroonne tegevus, seega kasutame await.

            var result = await _spaceshipServices.Create(dto);

            if (result == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
