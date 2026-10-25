using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Models.Cocktail
{
    public class CocktailViewModel
    {
        public string Drink { get; set; } = string.Empty;
        public string? IdDrink { get; set; }
        public string? Category { get; set; }
        public string? Alcoholic { get; set; }
        public string? Glass { get; set; }
        public string? Instructions { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Ingredients { get; set; } = [];

        public static CocktailViewModel FromDto(CocktailDto dto)
        {
            // Keep each measure paired with its numbered ingredient, even when slots are empty.
            (string? Ingredient, string? Measure)[] ingredients =
            [
                (dto.StrIngredient1, dto.StrMeasure1),
                (dto.StrIngredient2, dto.StrMeasure2),
                (dto.StrIngredient3, dto.StrMeasure3),
                (dto.StrIngredient4, dto.StrMeasure4),
                (dto.StrIngredient5, dto.StrMeasure5),
                (dto.StrIngredient6, dto.StrMeasure6),
                (dto.StrIngredient7, dto.StrMeasure7),
                (dto.StrIngredient8, dto.StrMeasure8),
                (dto.StrIngredient9, dto.StrMeasure9),
                (dto.StrIngredient10, dto.StrMeasure10),
                (dto.StrIngredient11, dto.StrMeasure11),
                (dto.StrIngredient12, dto.StrMeasure12),
                (dto.StrIngredient13, dto.StrMeasure13),
                (dto.StrIngredient14, dto.StrMeasure14),
                (dto.StrIngredient15, dto.StrMeasure15)
            ];

            return new CocktailViewModel
            {
                Drink = dto.StrDrink ?? string.Empty,
                IdDrink = dto.IdDrink,
                Category = dto.StrCategory,
                Alcoholic = dto.StrAlcoholic,
                Glass = dto.StrGlass,
                Instructions = dto.StrInstructions,
                ImageUrl = dto.StrDrinkThumb,
                Ingredients = ingredients
                    .Where(item => !string.IsNullOrWhiteSpace(item.Ingredient))
                    .Select(item => $"{item.Measure?.Trim()} {item.Ingredient!.Trim()}".Trim())
                    .ToList()
            };
        }
    }
}
