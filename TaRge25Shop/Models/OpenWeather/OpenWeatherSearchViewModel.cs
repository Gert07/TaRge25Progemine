using System.ComponentModel.DataAnnotations;

namespace TaRge25Shop.Models.OpenWeather
{
    public class OpenWeatherSearchViewModel
    {
        public const int MaxCityLength = 100;

        [Required(ErrorMessage = "Sisesta linna nimi.")]
        [StringLength(MaxCityLength, ErrorMessage = "Linna nimi võib olla kuni 100 märki pikk.")]
        [Display(Name = "Linn")]
        public string City { get; set; } = string.Empty;
    }
}
