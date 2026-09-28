using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class FileServices : IFileServices
    {
        private readonly IHostEnvironment _webHost;
        private readonly TaRge25ShopContext _context;

        public FileServices(
            IHostEnvironment webHost,
            TaRge25ShopContext context)
        {
            _webHost = webHost;
            _context = context;
        }

        public async Task FilesToApi(SpaceshipDto dto, Spaceship domain)
        {
            if (dto.Files == null || dto.Files.Count == 0)
            {
                return;
            }

            var uploadsFolder = Path.Combine(
                _webHost.ContentRootPath,
                "wwwroot",
                "multipleFileUpload");

            Directory.CreateDirectory(uploadsFolder);

            foreach (var file in dto.Files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                var originalFileName = Path.GetFileName(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var image = new FileToApi
                {
                    Id = Guid.NewGuid(),
                    ExistingFilePath = uniqueFileName,
                    SpaceshipId = domain.Id
                };

                _context.FileToApi.Add(image);
            }
        }

        public async Task<List<FileToApi>> RemoveImagesFromApi(FileToApiDto[] dtos)
        {
            if (dtos.Length == 0)
            {
                return new List<FileToApi>();
            }

            var imageIds = dtos.Select(x => x.Id).ToArray();
            var images = await _context.FileToApi
                .Where(x => imageIds.Contains(x.Id))
                .ToListAsync();

            foreach (var image in images)
            {
                DeletePhysicalFile(image.ExistingFilePath);
            }

            _context.FileToApi.RemoveRange(images);
            await _context.SaveChangesAsync();

            return images;
        }

        public async Task<FileToApi?> RemoveImageFromApi(FileToApiDto dto)
        {
            var images = await RemoveImagesFromApi(new[] { dto });
            return images.FirstOrDefault();
        }

        private void DeletePhysicalFile(string? existingFilePath)
        {
            var fileName = Path.GetFileName(existingFilePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var webRoot = Path.Combine(_webHost.ContentRootPath, "wwwroot");
            var filePaths = new[]
            {
                Path.Combine(webRoot, "multipleFileUpload", fileName),
                // Older uploads were saved directly in wwwroot with this prefix.
                Path.Combine(webRoot, "multipleFileUpload" + fileName)
            };

            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
