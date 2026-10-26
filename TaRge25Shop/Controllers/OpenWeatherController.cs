using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Models.OpenWeather;
using TaRge25Shop.Infrastructure;

namespace TaRge25Shop.Controllers
{
    public class OpenWeatherController : Controller
    {
        private readonly IOpenWeatherServices _openWeatherServices;

        public OpenWeatherController(IOpenWeatherServices openWeatherServices)
        {
            _openWeatherServices = openWeatherServices;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new OpenWeatherSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchWeather(OpenWeatherSearchViewModel model)
        {
            ValidateCity(model.City);
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            return RedirectToAction(nameof(Weather), new { city = model.City.Trim() });
        }

        [HttpGet]
        public async Task<IActionResult> Weather(string? city, CancellationToken cancellationToken = default)
        {
            var search = new OpenWeatherSearchViewModel { City = city?.Trim() ?? string.Empty };
            ValidateCity(search.City);
            if (!ModelState.IsValid)
            {
                return View("Index", search);
            }

            try
            {
                var weather = await _openWeatherServices.GetCurrentWeatherAsync(search.City, cancellationToken);
                var condition = weather.Weather![0];
                return View(new OpenWeatherViewModel
                {
                    City = weather.Name ?? search.City,
                    Country = weather.Sys?.Country,
                    Description = EstonianWeatherText.Describe(condition.Description, condition.Main),
                    IconUrl = string.IsNullOrWhiteSpace(condition.Icon) ? null
                        : $"https://openweathermap.org/img/wn/{Uri.EscapeDataString(condition.Icon)}@2x.png",
                    Temperature = weather.Main!.Temperature!.Value,
                    FeelsLike = weather.Main.FeelsLike,
                    Humidity = weather.Main.Humidity,
                    Pressure = weather.Main.Pressure,
                    WindSpeed = weather.Wind?.Speed,
                    Cloudiness = weather.Clouds?.All
                });
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, "Ilmateenus pole veel seadistatud. Lisa OpenWeather API võti.");
            }
            catch (HttpRequestException exception)
            {
                var message = exception.StatusCode switch
                {
                    HttpStatusCode.NotFound => "Linna ei leitud. Kontrolli nime või lisa riigikood, näiteks Tallinn,EE.",
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "OpenWeather API võti on vigane või pole veel aktiveeritud.",
                    HttpStatusCode.TooManyRequests => "Ilmateenuse päringute limiit on täis. Proovi hiljem uuesti.",
                    _ => "Ilmateenus pole praegu kättesaadav. Proovi hiljem uuesti."
                };
                ModelState.AddModelError(string.Empty, message);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                ModelState.AddModelError(string.Empty, "Ilmateenuse päring aegus. Proovi uuesti.");
            }
            catch (JsonException)
            {
                ModelState.AddModelError(string.Empty, "Ilmateenus tagastas vigased andmed. Proovi hiljem uuesti.");
            }

            return View("Index", search);
        }

        private void ValidateCity(string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                ModelState.TryAddModelError(nameof(OpenWeatherSearchViewModel.City), "Sisesta linna nimi.");
            }
            else if (city.Length > OpenWeatherSearchViewModel.MaxCityLength)
            {
                ModelState.TryAddModelError(nameof(OpenWeatherSearchViewModel.City), "Linna nimi võib olla kuni 100 märki pikk.");
            }
        }
    }
}
