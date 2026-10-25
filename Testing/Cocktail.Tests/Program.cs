using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaRge25Shop.ApplicationServices.Services;
using TaRge25Shop.Controllers;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Models.Cocktail;

var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
    Console.WriteLine("PASS: " + message);
}

var api = new CocktailApiHandler();
using var apiClient = new HttpClient(api);
var service = new CocktailServices(apiClient);
var drink = await service.SearchByNameAsync("  Gin & Tonic + lime?#  ");
Check(api.LastUri!.Query == "?s=Gin%20%26%20Tonic%20%2B%20lime%3F%23",
    "Search names are trimmed and URL-encoded without injecting query parameters");
Check(drink?.IdDrink == "11007" && drink.StrGlass == "Cocktail glass", "API fields deserialize and the first matching drink is selected");
var recipe = CocktailViewModel.FromDto(drink!);
Check(recipe.Ingredients.SequenceEqual(new[] { "1 oz Tequila", "Lime juice", "2 dashes Bitters" }),
    "Numbered ingredients and measures stay paired through gaps, including slot 15 and absent measures");
Check(await service.SearchByNameAsync("unknown") == null && await service.SearchByNameAsync("empty") == null,
    "Null and empty drink lists return no match");

try
{
    await service.SearchByNameAsync("   ");
    throw new Exception("Blank search unexpectedly succeeded");
}
catch (ArgumentException) { Check(true, "Service rejects blank searches"); }

try
{
    await service.SearchByNameAsync("unavailable");
    throw new Exception("HTTP error unexpectedly succeeded");
}
catch (HttpRequestException) { Check(true, "Non-success API responses are rejected"); }

try
{
    await service.SearchByNameAsync("bad-json");
    throw new Exception("Invalid JSON unexpectedly succeeded");
}
catch (JsonException) { Check(true, "Malformed API responses are rejected"); }

using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    try
    {
        await service.SearchByNameAsync("Margarita", canceled.Token);
        throw new Exception("Canceled request unexpectedly succeeded");
    }
    catch (OperationCanceledException) { Check(true, "Request cancellation is respected"); }
}

// Exercise real MVC routing, compiled Razor views, model binding, and antiforgery.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(CocktailController).Assembly.FullName,
    EnvironmentName = "Development"
});
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(CocktailController).Assembly);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<ICocktailServices>(service);
await using var app = builder.Build();
app.UseDeveloperExceptionPage();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var browserHandler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
    using var browser = new HttpClient(browserHandler) { BaseAddress = new Uri(address) };

    using var indexResponse = await browser.GetAsync("/Cocktail");
    var index = await indexResponse.Content.ReadAsStringAsync();
    if (!indexResponse.IsSuccessStatusCode) throw new Exception(index);
    var tokenMatch = Regex.Match(index, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
    Check(index.Contains("Cocktail name") && index.Contains("href=\"/Cocktail\"") && tokenMatch.Success,
        "Search page renders with a navigation link and antiforgery token");
    var token = WebUtility.HtmlDecode(tokenMatch.Groups[1].Value);

    async Task<HttpResponseMessage> Search(string name, bool withToken = true)
    {
        var fields = new Dictionary<string, string> { ["CocktailName"] = name };
        if (withToken) fields["__RequestVerificationToken"] = token;
        using var form = new FormUrlEncodedContent(fields);
        return await browser.PostAsync("/Cocktail/SearchCocktail", form);
    }

    using var valid = await Search("  Margarita  ");
    Check(valid.StatusCode == HttpStatusCode.Redirect
        && valid.Headers.Location!.OriginalString == "/Cocktail/Cocktail?cocktail=Margarita",
        "Valid form submission redirects to a bookmarkable recipe URL with a trimmed name");

    var details = await browser.GetStringAsync(valid.Headers.Location);
    Check(details.Contains("<h1>Margarita</h1>") && details.Contains("Cocktail glass")
        && details.Contains("Shake and serve.") && details.Contains("/images/margarita.jpg")
        && details.Contains("1 oz Tequila") && details.Contains("2 dashes Bitters")
        && !details.Contains("ignored measure"), "Recipe view renders photo, metadata, instructions, and nonempty ingredients");

    var beforeInvalid = api.Calls;
    using var blank = await Search("   ");
    Check(blank.StatusCode == HttpStatusCode.OK && (await blank.Content.ReadAsStringAsync()).Contains("Enter a cocktail name."),
        "Blank form submission redisplays field validation");
    using var tooLong = await Search(new string('a', 101));
    Check((await tooLong.Content.ReadAsStringAsync()).Contains("100 characters or fewer"), "Overlong form input is rejected");
    var missing = await browser.GetStringAsync("/Cocktail/Cocktail");
    var longUrl = await browser.GetStringAsync("/Cocktail/Cocktail?cocktail=" + new string('a', 101));
    Check(missing.Contains("Enter a cocktail name.") && longUrl.Contains("100 characters or fewer") && api.Calls == beforeInvalid,
        "Invalid direct URLs are handled without calling the API");
    using var noToken = await Search("Margarita", withToken: false);
    Check(noToken.StatusCode == HttpStatusCode.BadRequest, "POST searches require an antiforgery token");

    var noMatch = await browser.GetStringAsync("/Cocktail/Cocktail?cocktail=unknown");
    Check(noMatch.Contains("No cocktails found") && noMatch.Contains("value=\"unknown\""),
        "No matches display a helpful message and preserve the search name");
    foreach (var name in new[] { "unavailable", "bad-json", "null-json", "timeout" })
    {
        var failure = await browser.GetStringAsync("/Cocktail/Cocktail?cocktail=" + name);
        Check(failure.Contains("The cocktail service is unavailable.") && failure.Contains($"value=\"{name}\""),
            $"API failure '{name}' renders a retryable search form");
    }
    Console.WriteLine($"All {checks} cocktail checks passed.");
}
finally
{
    await app.StopAsync();
}

sealed class CocktailApiHandler : HttpMessageHandler
{
    public Uri? LastUri { get; private set; }
    public int Calls { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastUri = request.RequestUri;
        Calls++;
        var name = Uri.UnescapeDataString(request.RequestUri!.Query[3..]);
        if (name == "timeout") throw new TaskCanceledException("Simulated upstream timeout");
        var response = name switch
        {
            "unavailable" => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            "unknown" => Json("{\"drinks\":null}"),
            "empty" => Json("{\"drinks\":[]}"),
            "bad-json" => Json("not json"),
            "null-json" => Json("null"),
            _ => Json("""
                {"drinks":[{
                    "idDrink":"11007", "strDrink":"Margarita", "strCategory":"Ordinary Drink",
                    "strAlcoholic":"Alcoholic", "strGlass":"Cocktail glass",
                    "strInstructions":"Shake and serve.", "strDrinkThumb":"https://www.thecocktaildb.com/images/margarita.jpg",
                    "strIngredient1":" Tequila ", "strMeasure1":" 1 oz ",
                    "strIngredient2":" ", "strMeasure2":"ignored measure",
                    "strIngredient3":"Lime juice", "strMeasure3":null,
                    "strIngredient15":"Bitters", "strMeasure15":"2 dashes"
                }, {"idDrink":"second", "strDrink":"Another Margarita"}]}
                """)
        };
        return Task.FromResult(response);
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
    };
}
