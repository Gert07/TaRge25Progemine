using System.Text.Json;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class ChuckNorrisServices : IChuckNorrisServices
    {
        private readonly HttpClient _httpClient;

        public ChuckNorrisServices
            (
                HttpClient httpClient
            )
        {
            _httpClient = httpClient;
        }
        public async Task<ChuckNorrisRootDto> ChuckNorrisResultHttpClient()
        {
            //var response on vaja ühendada HttpClientiga, et saada vastus Chuck Norris API-st

            var response = await _httpClient.GetAsync("https://api.chucknorris.io/jokes/random");

            //annab veateate, kui response ei ole edukas 

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                // Olek true laseb ignoreerida JSON-i võtmete suuruse erinevusi, mis tähendab, et JSON-i võtmed ei pea olema
                // täpselt samasugused kui C#-i omaduste nimed. See on kasulik, kui JSON-i võtmed võivad olla erineva suurusega (näiteks "id" vs "Id").
                PropertyNameCaseInsensitive = true
            };

            // muutuja var joke tuleb desarieliseerida

            var joke = JsonSerializer.Deserialize<ChuckNorrisRootDto>(json, options);

            return joke; // Return the deserialized joke or throw an exception if needed
        }
    }
}
