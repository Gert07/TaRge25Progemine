using Microsoft.Extensions.Hosting;
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;


namespace TaRge25Shop.ApplicationServices.Services
{
    public class FileServices : IFileServices
    {
        //make constructor
        private readonly IHostEnvironment _webHost;
        private readonly TaRge25ShopContext _context;
        public FileServices 
            (
                IHostEnvironment webHost,
                TaRge25ShopContext context
            )
        {
            _webHost = webHost;
            _context = context;
        }
       public void FilesToApi(SpaceshipDto dto, Spaceship domain)
       {
            if (dto.Files != null && dto.Files.Count > 0)
            {
                //kui directoryt ei ole olemas, siis tee Directory
                // \\wwwroot\\multipleFileUpload\\
                // if

                if (!Directory.Exists(_webHost.ContentRootPath + "\\wwwroot\\multipleFileUpload\\"))
                {
                    Directory.CreateDirectory(_webHost.ContentRootPath + "\\wwwroot\\multipleFileUpload\\");
                }

                foreach (var file in dto.Files)
                {
                    //tuleb teha muutuja, kus on failide asukoht e kuhu hakatakse salvestama 
                    string uploadsFolder = Path.Combine(_webHost.ContentRootPath, "wwwroot", "multipleFileUpload");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    //tuleb kaks ülevalpool olevat muutujat kombineerida üheks
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    FileToApi path = new FileToApi
                    {
                        Id = Guid.NewGuid(),
                        ExistingFilePath = uniqueFileName,
                        SpaceshipId = domain.Id
                    };

                    _context.FileToApi.AddAsync(path);
                }
            }
       }
    }
}
