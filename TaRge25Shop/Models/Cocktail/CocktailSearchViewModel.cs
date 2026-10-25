using System.ComponentModel.DataAnnotations;

namespace TaRge25Shop.Models.Cocktail
{
    public class CocktailSearchViewModel
    {
        public const int MaxNameLength = 100;

        [Required(ErrorMessage = "Enter a cocktail name.")]
        [StringLength(MaxNameLength, ErrorMessage = "Use a cocktail name of 100 characters or fewer.")]
        [Display(Name = "Cocktail name")]
        public string CocktailName { get; set; } = string.Empty;

        public string? Message { get; set; }
    }
}
