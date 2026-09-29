using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Data;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Models.RealEstate;

namespace TaRge25Shop.Controllers
{
    public class RealEstateController : Controller
    {
        private readonly IRealEstateServices _realEstateServices;
        private readonly TaRge25ShopContext _context;

        public RealEstateController
            (
                IRealEstateServices realEstateServices,
                TaRge25ShopContext context
            )
        {
            _realEstateServices = realEstateServices;
            _context = context;
        }

        public IActionResult Index()
        {

            var result = _context.RealEstates
                .Select(x => new RealEstateIndexVM
                {
                    Id = x.Id,
                    Area = x.Area,
                    Location = x.Location,
                    CreatedAt = x.CreatedAt,
                    BuildingType = x.BuildingType
                });
            return View(result);
        }
        [HttpGet]
        public IActionResult Create()
        {
            RealEstateCreateUpdateVM result = new();

            return View("CreateUpdate", result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RealEstateCreateUpdateVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateUpdate", vm);
            }

            var dto = new RealEstateDto
            {
                Area = vm.Area,
                Location = vm.Location,
                RoomNumber = vm.RoomNumber,
                BuildingType = vm.BuildingType
            };

            var result = await _realEstateServices.Create(dto);

            if (result == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var realEstate = await _realEstateServices.DetailAsync(id);

            if (realEstate == null)
            {
                return NotFound();
            }
            var vm = new RealEstateCreateUpdateVM
            {
                Id = realEstate.Id,
                Area = realEstate.Area,
                Location = realEstate.Location,
                RoomNumber = realEstate.RoomNumber,
                BuildingType = realEstate.BuildingType,
                CreatedAt = realEstate.CreatedAt,
                ModifiedAt = realEstate.ModifiedAt
            };

            return View("CreateUpdate", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(RealEstateCreateUpdateVM vm)
        {
            if (!vm.Id.HasValue)
            {
                ModelState.AddModelError(string.Empty, "The real estate ID is missing.");
            }

            if (!ModelState.IsValid)
            {
                return View("CreateUpdate", vm);
            }

            var dto = new RealEstateDto
            {
                Id = vm.Id,
                Area = vm.Area,
                Location = vm.Location,
                RoomNumber = vm.RoomNumber,
                BuildingType = vm.BuildingType,
                CreatedAt = vm.CreatedAt,
                ModifiedAt = vm.ModifiedAt
            };

            var result = await _realEstateServices.Update(dto);
            if (result == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var realEstate = await _realEstateServices.DetailAsync(id);

            if (realEstate == null)
            {
                return NotFound();
            }

            var vm = new RealEstateDeleteVM

            {
                Id = realEstate.Id,
                Area = realEstate.Area,
                Location = realEstate.Location,
                RoomNumber = realEstate.RoomNumber,
                BuildingType = realEstate.BuildingType,
                CreatedAt = realEstate.CreatedAt,
                ModifiedAt = realEstate.ModifiedAt
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmation(Guid id)
        {
            var realEstate = await _realEstateServices.Delete(id);

            if (realEstate == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var realEstate = await _realEstateServices.DetailAsync(id);

            if (realEstate == null)
            {
                return NotFound();
            }
            var vm = new RealEstateDetailVM
            {
                Id = realEstate.Id,
                Area = realEstate.Area,
                Location = realEstate.Location,
                RoomNumber = realEstate.RoomNumber,
                BuildingType = realEstate.BuildingType,
                CreatedAt = realEstate.CreatedAt,
                ModifiedAt = realEstate.ModifiedAt
            };

            return View(vm);
        }
    } 
}
