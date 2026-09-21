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
        private readonly IFileServices _fileServices;

        public SpaceshipController
            (
                ISpaceshipServices spaceshipServices,
                TaRge25ShopContext context,
                IFileServices fileServices
            )
        {
            _spaceshipServices = spaceshipServices;
            _context = context;
            _fileServices = fileServices;
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
            SpaceshipCreateUpdateViewModel result = new();

            return View("CreateUpdate", result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(SpaceshipCreateUpdateViewModel vm)
        {
            var dto = new SpaceshipDto
            {
                Name = vm.Name,
                ShipType = vm.ShipType,
                Crew = vm.Crew,
                EnginePower = vm.EnginePower,
                Files = vm.Files,
                FileToApiDtos = vm.Image
                    .Select(x => new FileToApiDto
                    {
                        Id = x.ImageId,
                        ExistingFilePath = x.FilePath,
                        SpaceshipId = x.SpaceshipId
                    }).ToArray()
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


        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var spaceship = await _spaceshipServices.DetailAsync(id);

            if (spaceship == null)
            {
                return NotFound();
            }
            var vm = new SpaceshipCreateUpdateViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                CreatedAt = spaceship.CreatedAt,
                UpdatedAt = spaceship.UpdatedAt
            };

            return View("CreateUpdate", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(SpaceshipCreateUpdateViewModel vm)
        {
            var dto = new SpaceshipDto
            {
                Id = vm.Id,
                Name = vm.Name,
                ShipType = vm.ShipType, 
                Crew = vm.Crew,
                EnginePower = vm.EnginePower,
                CreatedAt = vm.CreatedAt,
                UpdatedAt = vm.UpdatedAt
            };

            var result = await _spaceshipServices.Update(dto);
            if (result == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var spaceship = await _spaceshipServices.DetailAsync(id);

            if (spaceship == null)
            {
                return NotFound();
            }

            //tuleb teha vaheinstants dto ja vm vahel
            var vm = new SpaceshipDeleteViewModel

                {
                    Id = spaceship.Id,
                    Name = spaceship.Name,
                    ShipType = spaceship.ShipType,
                    CreatedAt = spaceship.CreatedAt,
                    Crew = spaceship.Crew,
                    EnginePower = spaceship.EnginePower,
                    UpdatedAt = spaceship.UpdatedAt
                };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmation(Guid id)
        {
            var spaceship = await _spaceshipServices.Delete(id);
            
            if (spaceship == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        //thea detaili vaade
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var spaceship = await _spaceshipServices.DetailAsync(id);

            if (spaceship == null)
            {
                return NotFound();
            }
            var vm = new SpaceshipDetailViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                CreatedAt = spaceship.CreatedAt,
                UpdatedAt = spaceship.UpdatedAt
            };

            return View(vm);
        }
    }
}
