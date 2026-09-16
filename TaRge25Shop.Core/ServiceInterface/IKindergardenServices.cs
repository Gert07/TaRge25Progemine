using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Core.ServiceInterface
{
    public interface IKindergardenServices
    {
        Task<Kindergarden> Create(Kindergarden dto);
        Task<Kindergarden> Update(Kindergarden dto);
        Task<Kindergarden> DetailAsync(Guid id);
        Task<Kindergarden> Delete(Guid id);
    }
}
