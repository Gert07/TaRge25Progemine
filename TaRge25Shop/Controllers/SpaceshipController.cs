using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Models.Spaceship;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;

namespace TaRge25Shop.Controllers
{
    public class SpaceshipController : Controller
    {
        private readonly ISpaceshipServices _spaceshipServices;
        private readonly TaRge25ShopContext _context;

        public SpaceshipController
            (
                ISpaceshipServices spaceshipServices,
                TaRge25ShopContext context
            )
        {
            _spaceshipServices = spaceshipServices;
            _context = context;
        }

        public IActionResult Index()
        {
            
            var result = _context.Spaceships
                .Select(x => new SpaceshipIndexViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ShipType = x.ShipType,
                    CreatedAt = x.CreatedAt,
                    Crew = x.Crew
                });
            //Kutsume teenuse välja, et saada kõik kosmoselaevad
            //See on asünkroonne tegevus ja kasutame await.
            //constructoris tuleb välja kutsuda DbContext, et saaksime andmed kätte.
            //Seejärel kutsume andmed välja
            return View(result);
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

        public async Task<IActionResult> Update(Guid id)
        {
            var spaceship = await _spaceshipServices.DetailAsync(id);

            if (spaceship == null)
            {
                return NotFound();
            }
            var vm = new SpaceshipUpdateViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower
            };

            return View(vm);
        }
    }
}
