using System.ComponentModel.DataAnnotations;

namespace TaRge25Shop.Models.Cocktail
{
    public class CocktailSearchViewModel
    {
        public const int MaxNameLength = 100;

        [Required(ErrorMessage = "Sisesta kokteili nimi.")]
        [StringLength(MaxNameLength, ErrorMessage = "Kokteili nimi võib olla kuni 100 märki pikk.")]
        [Display(Name = "Kokteili nimi")]
        public string CocktailName { get; set; } = string.Empty;

        public string? Message { get; set; }
    }
}
