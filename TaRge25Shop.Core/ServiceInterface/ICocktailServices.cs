using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Core.ServiceInterface
{
    public interface ICocktailServices
    {
        Task<CocktailDto?> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
