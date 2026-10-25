using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class OpenWeatherServices : IOpenWeatherServices
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenWeatherServices(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<OpenWeatherRootDto> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(city);
            var apiKey = _configuration["OpenWeather:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("OpenWeather API key is not configured.");
            }

            // Current Weather Data: metric returns Celsius and wind speed in m/s.
            var url = "https://api.openweathermap.org/data/2.5/weather?q="
                + Uri.EscapeDataString(city.Trim())
                + "&appid=" + Uri.EscapeDataString(apiKey.Trim())
                + "&units=metric";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var weather = await response.Content.ReadFromJsonAsync<OpenWeatherRootDto>(cancellationToken);
            if (weather?.Main?.Temperature == null || weather.Weather?.FirstOrDefault() is not { } condition
                || string.IsNullOrWhiteSpace(condition.Description ?? condition.Main))
            {
                throw new JsonException("OpenWeather returned incomplete current weather data.");
            }

            return weather;
        }
    }
}
