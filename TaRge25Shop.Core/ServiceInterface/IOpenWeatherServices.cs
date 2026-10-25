using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Core.ServiceInterface
{
    public interface IOpenWeatherServices
    {
        Task<OpenWeatherRootDto> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken = default);
    }
}
