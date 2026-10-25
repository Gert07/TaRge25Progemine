using System.Net.Http.Json;
using System.Text.Json;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class CocktailServices : ICocktailServices
    {
        private readonly HttpClient _httpClient;

        public CocktailServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CocktailDto?> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var url = "https://www.thecocktaildb.com/api/json/v1/1/search.php?s="
                + Uri.EscapeDataString(name.Trim());

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CocktailRootDto>(cancellationToken);
            if (result == null)
            {
                throw new JsonException("The cocktail API returned an empty response.");
            }

            return result.Cocktails?.FirstOrDefault();
        }
    }
}
