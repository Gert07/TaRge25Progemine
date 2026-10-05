
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Core.ServiceInterface
{
    public interface IFileServices
    {
        Task FilesToApi(SpaceshipDto dto, Spaceship domain);
        Task<List<FileToApi>> RemoveImagesFromApi(FileToApiDto[] dtos);
        Task<FileToApi?> RemoveImageFromApi(FileToApiDto dto);
        void UploadFilesToDatabase(RealEstateDto dto, RealEstate domain);
    }
}
