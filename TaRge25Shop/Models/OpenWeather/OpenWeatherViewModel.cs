using System.ComponentModel.DataAnnotations;

namespace TaRge25Shop.Models.OpenWeather
{
    public class OpenWeatherViewModel
    {
        public string City { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Ikooni aadress")]
        public string? IconUrl { get; set; }
        public double Temperature { get; set; }
        public double? FeelsLike { get; set; }
        public int? Humidity { get; set; }
        public int? Pressure { get; set; }
        public double? WindSpeed { get; set; }
        public int? Cloudiness { get; set; }
    }
}
