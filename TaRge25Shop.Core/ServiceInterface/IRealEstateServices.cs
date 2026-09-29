using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Core.ServiceInterface
{
    public interface IRealEstateServices
    {
        Task<RealEstate> Create(RealEstateDto dto);
        Task<RealEstate?> Update(RealEstateDto dto);
        Task<RealEstate?> DetailAsync(Guid id);
        Task<RealEstate?> Delete(Guid id);
    }
}
