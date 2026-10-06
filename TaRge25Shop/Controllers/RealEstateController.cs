using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Data;
using Microsoft.EntityFrameworkCore;
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
                BuildingType = vm.BuildingType,
                Files = vm.Files,
                Image = vm.Images
                    .Select(x => new FileToDatabaseDto
                {
                    Id = x.ImageId,
                    RealEstateId = x.RealEstateId,
                    ImageTitle = x.ImageTitle,
                    ImageData = x.ImageData
                }).ToArray()
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

            RealEstateImageVM[] images = await _context.FileToDatabases
                .AsNoTracking()
                .Where(x => x.RealEstateId == id && x.ImageData != null)
                .Select(x => new RealEstateImageVM
                {
                    ImageId = x.Id,
                    ImageTitle = x.ImageTitle,
                    RealEstateId = x.RealEstateId
                })
                .ToArrayAsync();

            var vm = new RealEstateCreateUpdateVM();

            vm.Id = realEstate.Id;
            vm.Area = realEstate.Area;
            vm.Location = realEstate.Location;
            vm.RoomNumber = realEstate.RoomNumber;
            vm.BuildingType = realEstate.BuildingType;
            vm.CreatedAt = realEstate.CreatedAt;
            vm.ModifiedAt = realEstate.ModifiedAt;
            vm.Images.AddRange(images);


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
                ModifiedAt = vm.ModifiedAt,
                Files = vm.Files
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
                ModifiedAt = realEstate.ModifiedAt,
                Images = await _context.FileToDatabases
                    .AsNoTracking()
                    .Where(x => x.RealEstateId == id && x.ImageData != null)
                    .Select(x => new RealEstateImageVM
                    {
                        ImageId = x.Id,
                        ImageTitle = x.ImageTitle,
                        RealEstateId = x.RealEstateId
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var image = await _context.FileToDatabases
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (image?.ImageData == null)
            {
                return NotFound();
            }

            return File(image.ImageData, GetImageContentType(image.ImageTitle));
        }

        private static string GetImageContentType(string? fileName)
        {
            return Path.GetExtension(fileName)?.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveImage(Guid imageId)
        {
            var image = await _context.FileToDatabases.FindAsync(imageId);

            if (image == null)
            {
                return NotFound();
            }

            var realEstateId = image.RealEstateId;

            _context.FileToDatabases.Remove(image);
            await _context.SaveChangesAsync();

            return realEstateId.HasValue
                ? RedirectToAction(nameof(Update), new { id = realEstateId.Value })
                : RedirectToAction(nameof(Index));
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
                ModifiedAt = realEstate.ModifiedAt,
                Images = await _context.FileToDatabases
                    .AsNoTracking()
                    .Where(x => x.RealEstateId == id && x.ImageData != null)
                    .Select(x => new RealEstateImageVM
                    {
                        ImageId = x.Id,
                        ImageTitle = x.ImageTitle,
                        RealEstateId = x.RealEstateId
                    })
                    .ToListAsync()
            };

            return View(vm);
        }
    } 
}
