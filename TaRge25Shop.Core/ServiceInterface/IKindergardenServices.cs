using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Core.ServiceInterface
{
    public interface IKindergardenServices
    {
        Task<Kindergarden> Create(KindergardenDto dto);
        Task<Kindergarden> Update(KindergardenDto dto);
        Task<Kindergarden> DetailAsync(Guid id);
        Task<Kindergarden> Delete(Guid id);
    }
}
