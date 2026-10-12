using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using TaRge25Shop.Data;
using Xunit;

namespace TaRge25Shop.SeleniumTesting
{
    public sealed class SeleniumWebAppFixture : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"SeleniumTests-{Guid.NewGuid()}";

        public SeleniumWebAppFixture()
        {
            // A real HTTP server is required because Firefox cannot connect to
            // WebApplicationFactory's default in-memory TestServer.
            UseKestrel(0);
            StartServer();
        }

        public Uri BaseAddress => ClientOptions.BaseAddress;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<TaRge25ShopContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<TaRge25ShopContext>>();
                services.RemoveAll<TaRge25ShopContext>();
                services.AddDbContext<TaRge25ShopContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    public class SpaceshipFrontendTests : IClassFixture<SeleniumWebAppFixture>
    {
        private readonly SeleniumWebAppFixture _webApp;

        public SpaceshipFrontendTests(SeleniumWebAppFixture webApp)
        {
            _webApp = webApp;
        }

        [Fact]
        public void Should_NavigateToCreate_AddSpaceShip_WithCorrectData_ReturnToIndex()
        {
            using IWebDriver driver = CreateFirefoxDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            driver.Navigate().GoToUrl(_webApp.BaseAddress);
            NavigateToLink(driver, By.CssSelector("nav a[href='/Spaceship']"));
            NavigateToLink(driver, By.CssSelector("a[href='/Spaceship/Create']"));

            InsertSpaceShipData(driver);
        }

        private static IWebDriver CreateFirefoxDriver()
        {
            var driverService = FirefoxDriverService.CreateDefaultService(AppContext.BaseDirectory);
            var options = new FirefoxOptions
            {
                AcceptInsecureCertificates = true
            };
            options.AddArgument("-headless");

            return new FirefoxDriver(driverService, options);
        }

        private static void NavigateToLink(IWebDriver driver, By linkSelector)
        {
            var destination = driver.FindElement(linkSelector).GetAttribute("href");
            Assert.False(string.IsNullOrWhiteSpace(destination));
            driver.Navigate().GoToUrl(destination);
        }

        private static void InsertSpaceShipData(IWebDriver driver)
        {
            IWebElement cu_NameEntrySpaceship = driver.FindElement(By.Id("Name"));
            cu_NameEntrySpaceship.Clear();
            cu_NameEntrySpaceship.SendKeys("i add name for spaceship");

            IWebElement cu_ShipTypeEntrySpaceship = driver.FindElement(By.Id("ShipType"));
            cu_ShipTypeEntrySpaceship.Clear();
            cu_ShipTypeEntrySpaceship.SendKeys("i add ship type for spaceship");

            IWebElement cu_CrewEntrySpaceship = driver.FindElement(By.Id("Crew"));
            cu_CrewEntrySpaceship.Clear();
            cu_CrewEntrySpaceship.SendKeys("12");

            IWebElement cu_EnginePowerEntrySpaceship = driver.FindElement(By.Id("EnginePower"));
            cu_EnginePowerEntrySpaceship.Clear();
            cu_EnginePowerEntrySpaceship.SendKeys("2500");

            IWebElement cu_CreateSpaceship = driver.FindElement(By.CssSelector("input[type='submit'][value='Create Spaceship']"));
            cu_CreateSpaceship.Click();

            IWebElement createdRow = driver.FindElement(By.XPath(
                "//tbody/tr[td[normalize-space()='i add name for spaceship']]"));
            var cells = createdRow.FindElements(By.TagName("td"));

            Assert.Equal("i add name for spaceship", cells[1].Text);
            Assert.Equal("i add ship type for spaceship", cells[2].Text);
            Assert.Equal("12", cells[4].Text);
        }
    }
}
