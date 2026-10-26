using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaRge25Shop.ApplicationServices.Services;
using TaRge25Shop.Controllers;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Infrastructure;

var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
    Console.WriteLine("PASS: " + message);
}

var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["OpenWeather:ApiKey"] = "test-key"
}).Build();
var api = new WeatherApiHandler();
using var apiClient = new HttpClient(api);
var service = new OpenWeatherServices(apiClient, configuration);
var result = await service.GetCurrentWeatherAsync("  Pärnu & Tallinn,EE?#  ");
Check(api.LastUri!.AbsolutePath == "/data/2.5/weather"
    && api.LastUri.Query == "?q=P%C3%A4rnu%20%26%20Tallinn%2CEE%3F%23&appid=test-key&units=metric",
    "Current Weather Data request trims and encodes the city, includes the configured key and uses metric units");
Check(result.Name == "Tallinn" && result.Main?.Temperature == -2.5 && result.Main.FeelsLike == -6.1
    && result.Weather?[0].Description == "light snow" && result.Wind?.Speed == 3.2,
    "Nested API fields deserialize with negative Celsius temperatures and wind speed");

configuration["OpenWeather:ApiKey"] = "";
var beforeMissingKey = api.Calls;
try
{
    await service.GetCurrentWeatherAsync("Tallinn");
    throw new Exception("Missing API key unexpectedly succeeded");
}
catch (InvalidOperationException) { Check(api.Calls == beforeMissingKey, "Missing API key is rejected without a network call"); }
configuration["OpenWeather:ApiKey"] = "test-key";

using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    try
    {
        await service.GetCurrentWeatherAsync("Tallinn", canceled.Token);
        throw new Exception("Canceled API call unexpectedly succeeded");
    }
    catch (OperationCanceledException) { Check(true, "Request cancellation is respected"); }
}

// Use the real controller and compiled Razor views with deterministic API responses.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(OpenWeatherController).Assembly.FullName,
    EnvironmentName = "Development"
});
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddControllersWithViews().AddEstonianUi().AddApplicationPart(typeof(OpenWeatherController).Assembly);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<IOpenWeatherServices>(service);
await using var app = builder.Build();
app.UseDeveloperExceptionPage();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
    using var browser = new HttpClient(handler) { BaseAddress = new Uri(address) };
    async Task<string> Page(string path)
    {
        using var response = await browser.GetAsync(path);
        var body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        if (!response.IsSuccessStatusCode) throw new Exception(body);
        return body;
    }

    var index = await Page("/OpenWeather");
    var token = Regex.Match(index, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
    Check(index.Contains("Praegune ilm") && index.Contains("href=\"/OpenWeather\"") && token.Length > 0,
        "Weather search renders with navigation and an antiforgery token");
    async Task<HttpResponseMessage> Search(string city, bool withToken = true)
    {
        var fields = new Dictionary<string, string> { ["City"] = city };
        if (withToken) fields["__RequestVerificationToken"] = token;
        using var form = new FormUrlEncodedContent(fields);
        return await browser.PostAsync("/OpenWeather/SearchWeather", form);
    }

    using var search = await Search("  Tallinn,EE  ");
    Check(search.StatusCode == HttpStatusCode.Redirect
        && Uri.UnescapeDataString(search.Headers.Location!.OriginalString) == "/OpenWeather/Weather?city=Tallinn,EE",
        "Valid search redirects to a bookmarkable weather URL with a trimmed city");
    var weather = await Page(search.Headers.Location!.OriginalString);
    Check(weather.Contains("Tallinn, EE") && weather.Contains("light snow")
        && weather.Contains("https://openweathermap.org/img/wn/13d@2x.png")
        && weather.Contains("°C") && weather.Contains("m/s") && weather.Contains("hPa")
        && weather.Contains("Õhuniiskus") && weather.Contains("Pilvisus"),
        "Weather view renders city, conditions, icon, temperatures, humidity, wind, pressure and clouds");

    var beforeInvalid = api.Calls;
    using var blank = await Search("   ");
    Check(WebUtility.HtmlDecode(await blank.Content.ReadAsStringAsync()).Contains("Sisesta linna nimi."),
        "Blank form submission redisplays validation");
    using var overlong = await Search(new string('x', 101));
    Check(WebUtility.HtmlDecode(await overlong.Content.ReadAsStringAsync()).Contains("kuni 100 märki"),
        "Overlong form submission is rejected");
    Check((await Page("/OpenWeather/Weather")).Contains("Sisesta linna nimi.")
        && (await Page("/OpenWeather/Weather?city=" + new string('x', 101))).Contains("kuni 100 märki")
        && api.Calls == beforeInvalid, "Invalid direct URLs do not call the API");
    using var noToken = await Search("Tallinn", withToken: false);
    Check(noToken.StatusCode == HttpStatusCode.BadRequest, "Search form requires an antiforgery token");

    var failures = new Dictionary<string, string>
    {
        ["missing"] = "Linna ei leitud",
        ["unauthorized"] = "API võti on vigane",
        ["forbidden"] = "API võti on vigane",
        ["limited"] = "päringute limiit on täis",
        ["unavailable"] = "pole praegu kättesaadav",
        ["network"] = "pole praegu kättesaadav",
        ["timeout"] = "päring aegus",
        ["bad-json"] = "tagastas vigased andmed",
        ["incomplete"] = "tagastas vigased andmed",
        ["null-condition"] = "tagastas vigased andmed"
    };
    foreach (var (city, message) in failures)
    {
        var failure = await Page("/OpenWeather/Weather?city=" + city);
        Check(failure.Contains(message) && failure.Contains($"value=\"{city}\"") && !failure.Contains("test-key"),
            $"Failure '{city}' renders a helpful message, preserves input and hides the API key");
    }
    var optional = await Page("/OpenWeather/Weather?city=minimal");
    Check(optional.Contains("clear sky") && !optional.Contains("Õhuniiskus") && !optional.Contains("Tuule kiirus"),
        "Missing optional weather measurements are omitted rather than displayed as zero");
    configuration["OpenWeather:ApiKey"] = "";
    Check((await Page("/OpenWeather/Weather?city=Tallinn")).Contains("Lisa OpenWeather API võti"),
        "Missing configuration renders a useful setup message");
    Console.WriteLine($"All {checks} OpenWeather checks passed.");
}
finally { await app.StopAsync(); }

sealed class WeatherApiHandler : HttpMessageHandler
{
    public Uri? LastUri { get; private set; }
    public int Calls { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastUri = request.RequestUri;
        Calls++;
        var query = request.RequestUri!.Query;
        var city = Uri.UnescapeDataString(query[3..query.IndexOf('&')]);
        if (city == "timeout") throw new TaskCanceledException("Simulated timeout");
        if (city == "network") throw new HttpRequestException("Simulated connection failure");
        var response = city switch
        {
            "missing" => new(HttpStatusCode.NotFound),
            "unauthorized" => new(HttpStatusCode.Unauthorized),
            "forbidden" => new(HttpStatusCode.Forbidden),
            "limited" => new(HttpStatusCode.TooManyRequests),
            "unavailable" => new(HttpStatusCode.ServiceUnavailable),
            "bad-json" => Json("not JSON"),
            "incomplete" => Json("{\"name\":\"Tallinn\"}"),
            "null-condition" => Json("{\"main\":{\"temp\":0},\"weather\":[null]}"),
            "minimal" => Json("{\"name\":\"Tallinn\",\"main\":{\"temp\":0},\"weather\":[{\"description\":\"clear sky\"}]}"),
            _ => Json("""
                {"name":"Tallinn","sys":{"country":"EE"},
                 "weather":[{"main":"Snow","description":"light snow","icon":"13d"}],
                 "main":{"temp":-2.5,"feels_like":-6.1,"humidity":90,"pressure":1015},
                 "wind":{"speed":3.2},"clouds":{"all":75}}
                """)
        };
        return Task.FromResult(response);
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
    };
}
