using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Data;
using Microsoft.EntityFrameworkCore;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Models.Kindergarden;

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

            var result = _context.Kindergardens
                .Select(x => new KindergardenIndexViewModel
                {
                    Id = x.Id,
                    GroupName = x.GroupName,
                    ChildrenCount = x.ChildrenCount,
                    KindergardenName = x.KindergardenName,
                    CreatedAt = x.CreatedAt,
                    TeacherName = x.TeacherName
                });
            return View(result);
        }
        [HttpGet]
        public IActionResult Create()
        {
            KindergardenCreateUpdateViewModel result = new();

            return View("CreateUpdate", result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KindergardenCreateUpdateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateUpdate", vm);
            }

            var dto = new KindergardenDto
            {
                GroupName = vm.GroupName,
                KindergardenName = vm.KindergardenName,
                ChildrenCount = vm.ChildrenCount,
                TeacherName = vm.TeacherName,
                Files = vm.Files
            };

            await _kindergardenServices.Create(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var kindergarden = await _kindergardenServices.DetailAsync(id);

            if (kindergarden == null)
            {
                return NotFound();
            }

            var images = await LoadImages(id);

            var vm = new KindergardenCreateUpdateViewModel();

            vm.Id = kindergarden.Id;
            vm.GroupName = kindergarden.GroupName;
            vm.KindergardenName = kindergarden.KindergardenName;
            vm.ChildrenCount = kindergarden.ChildrenCount;
            vm.TeacherName = kindergarden.TeacherName;
            vm.CreatedAt = kindergarden.CreatedAt;
            vm.UpdatedAt = kindergarden.UpdatedAt;
            vm.Images.AddRange(images);


            return View("CreateUpdate", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(KindergardenCreateUpdateViewModel vm)
        {
            if (!vm.Id.HasValue)
            {
                ModelState.AddModelError(string.Empty, "The kindergarten ID is missing.");
            }

            if (!ModelState.IsValid)
            {
                if (vm.Id.HasValue)
                {
                    vm.Images = await LoadImages(vm.Id.Value);
                }
                return View("CreateUpdate", vm);
            }

            var dto = new KindergardenDto
            {
                Id = vm.Id,
                GroupName = vm.GroupName,
                KindergardenName = vm.KindergardenName,
                ChildrenCount = vm.ChildrenCount,
                TeacherName = vm.TeacherName,
                Files = vm.Files
            };

            var result = await _kindergardenServices.Update(dto);
            if (result == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var kindergarden = await _kindergardenServices.DetailAsync(id);

            if (kindergarden == null)
            {
                return NotFound();
            }

            var vm = new KindergardenDeleteViewModel

            {
                Id = kindergarden.Id,
                GroupName = kindergarden.GroupName,
                KindergardenName = kindergarden.KindergardenName,
                ChildrenCount = kindergarden.ChildrenCount,
                TeacherName = kindergarden.TeacherName,
                CreatedAt = kindergarden.CreatedAt,
                UpdatedAt = kindergarden.UpdatedAt,
                Images = await LoadImages(id)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var image = await _context.FileToDatabases
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.KindergardenId != null);

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

            if (image == null || !image.KindergardenId.HasValue)
            {
                return NotFound();
            }

            var kindergardenId = image.KindergardenId;

            _context.FileToDatabases.Remove(image);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Update), new { id = kindergardenId.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmation(Guid id)
        {
            await _kindergardenServices.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var kindergarden = await _kindergardenServices.DetailAsync(id);

            if (kindergarden == null)
            {
                return NotFound();
            }
            var vm = new KindergardenDetailViewModel
            {
                Id = kindergarden.Id,
                GroupName = kindergarden.GroupName,
                KindergardenName = kindergarden.KindergardenName,
                ChildrenCount = kindergarden.ChildrenCount,
                TeacherName = kindergarden.TeacherName,
                CreatedAt = kindergarden.CreatedAt,
                UpdatedAt = kindergarden.UpdatedAt,
                Images = await LoadImages(id)
            };

            return View(vm);
        }
        private Task<List<KindergardenImageViewModel>> LoadImages(Guid id)
        {
            return _context.FileToDatabases
                .AsNoTracking()
                .Where(x => x.KindergardenId == id && x.ImageData != null)
                .Select(x => new KindergardenImageViewModel
                {
                    ImageId = x.Id,
                    ImageTitle = x.ImageTitle,
                    KindergardenId = x.KindergardenId
                })
                .ToListAsync();
        }
    }
}
