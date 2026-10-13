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
using Xunit.Abstractions;
using Xunit.Sdk;

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

    public class OrderAttribute : Attribute
    {
        public int Value { get; }
        public OrderAttribute (int value)
        {
            Value = value;
        }
    }

    public class PriorityOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            return testCases.OrderBy(test =>
            {
                var method = test.TestMethod.Method.GetCustomAttributes(typeof(OrderAttribute)).FirstOrDefault();
                if (method == null)
                {
                    return int.MaxValue;
                }
                else
                {
                    return (int)method.GetConstructorArguments().Single();
                }
            }).ToList();
        }
    }


    [TestCaseOrderer("TaRge25Shop.SeleniumTesting.PriorityOrderer", "TaRge25Shop.SeleniumTesting")]
    public class SpaceshipFrontendTests : IClassFixture<SeleniumWebAppFixture>
    {
        private readonly SeleniumWebAppFixture _webApp;

        public SpaceshipFrontendTests(SeleniumWebAppFixture webApp)
        {
            _webApp = webApp;

            // Each test creates its own spaceship and can run independently.
            using var scope = _webApp.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TaRge25ShopContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [Fact]
        public void Should_NavigateToCreate_AddSpaceShip_WithCorrectData_ReturnToIndex()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();
            //tegevus
            IWebElement createInIndex = driver.FindElement(By.LinkText("Create Spaceship"));
            createInIndex.Click();
            Assert.Equal("/Spaceship/Create", new Uri(driver.Url).AbsolutePath);

            //sisestatavad andmed
            InsertSpaceShipData(driver);
            IWebElement createButton = driver.FindElement(By.CssSelector("input[type='submit'][value='Create Spaceship']"));
            createButton.Click();

            //andmete kogumine pärast reloadi
            Assert.Equal("/Spaceship", new Uri(driver.Url).AbsolutePath);
            var createdRow = FindSpaceshipRow(driver, "i add name for spaceship");
            var cells = createdRow.FindElements(By.TagName("td"));
            //kontroll
            Assert.Equal("i add name for spaceship", cells[1].Text);
            Assert.Equal("i add ship type for spaceship", cells[2].Text);
            Assert.Equal("12345", cells[4].Text);
        }

        [Fact]
        public void Should_NavigateToDetails_OfASpaceShip_WithPreviouslyCorrectData()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();
            var createdRow = CreateSpaceship(driver);
            //siis vajuta selles reas asuvat details nuppu
            IWebElement detailsButton = createdRow.FindElement(By.LinkText("Details"));
            detailsButton.Click();

            //kontroll
            Assert.StartsWith("/Spaceship/Details/", new Uri(driver.Url).AbsolutePath);
            //andmete kogumine pärast reloadi
            var id = driver.FindElement(By.CssSelector("input#Id.form-control-plaintext")).GetAttribute("value");
            //kontroll
            Assert.True(Guid.TryParse(id, out var spaceshipId));
            Assert.NotEqual(Guid.Empty, spaceshipId);
            Assert.Equal("i add name for spaceship", driver.FindElement(By.Id("Name")).GetAttribute("value"));
            Assert.Equal("i add ship type for spaceship", driver.FindElement(By.Id("ShipType")).GetAttribute("value"));
            Assert.Equal("12345", driver.FindElement(By.Id("Crew")).GetAttribute("value"));
            Assert.Equal("666667", driver.FindElement(By.Id("EnginePower")).GetAttribute("value"));
        }

        [Fact]
        public void Should_UpdateDetails_OfASpaceShip_WithNewData()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();
            var createdRow = CreateSpaceship(driver);
            //vajuta selles reas asuvat update nuppu
            IWebElement updateButton = createdRow.FindElement(By.LinkText("Update"));
            updateButton.Click();
            var spaceshipId = driver.FindElement(By.Id("Id")).GetAttribute("value");

            //sisestatavad andmed
            InsertSpaceShipData(driver, true);
            IWebElement saveButton = driver.FindElement(By.CssSelector("input[type='submit'][value='Update Spaceship']"));
            saveButton.Click();

            //kontroll
            Assert.Equal("/Spaceship", new Uri(driver.Url).AbsolutePath);
            //andmete kogumine pärast reloadi
            var updatedRow = FindSpaceshipRow(driver, "DISISENJUNAEM");
            var cells = updatedRow.FindElements(By.TagName("td"));
            //kontroll
            Assert.Equal("DISISENJUNAEM", cells[1].Text);
            Assert.Equal("completely unheard of haunebu", cells[2].Text);
            Assert.Equal("67890", cells[4].Text);
            Assert.Empty(driver.FindElements(By.XPath("//tbody/tr[td[normalize-space()='i add name for spaceship']]")));

            //kontrollime salvestatud andmeid ka detailvaates
            IWebElement detailsButton = updatedRow.FindElement(By.LinkText("Details"));
            detailsButton.Click();
            Assert.Equal(spaceshipId, driver.FindElement(By.CssSelector("input#Id.form-control-plaintext")).GetAttribute("value"));
            Assert.Equal("DISISENJUNAEM", driver.FindElement(By.Id("Name")).GetAttribute("value"));
            Assert.Equal("completely unheard of haunebu", driver.FindElement(By.Id("ShipType")).GetAttribute("value"));
            Assert.Equal("67890", driver.FindElement(By.Id("Crew")).GetAttribute("value"));
            Assert.Equal("444", driver.FindElement(By.Id("EnginePower")).GetAttribute("value"));
        }

        private IWebDriver SetupAndNavigateSIndex()
        {
            //firefoxi käskiv ja juhtiv draiver
            var driver = CreateFirefoxDriver();
            //elementide leidmisel ootab draiver vajadusel kuni viis sekundit
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            try
            {
                //aadress millele draiver navigeerib
                driver.Navigate().GoToUrl(_webApp.BaseAddress);
                //lehelt otsitav element ja sellel klõpsamine
                IWebElement navigateToSpaceship = driver.FindElement(By.LinkText("Spaceship"));
                navigateToSpaceship.Click();
                Assert.Equal("/Spaceship", new Uri(driver.Url).AbsolutePath);
                return driver;
            }
            catch
            {
                driver.Dispose();
                throw;
            }
        }
        [Fact]
        public void Should_UpdateDetails_OfASpaceship_WithNewData()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();
            var createdRow = CreateSpaceship(driver);
            //vajuta selles reas asuvat update nuppu
            IWebElement updateButton = createdRow.FindElement(By.LinkText("Update"));
            updateButton.Click();

            //kontrollime vormi eeltäidetud andmeid
            Assert.Equal("i add name for spaceship", driver.FindElement(By.Id("Name")).GetAttribute("value"));
            Assert.Equal("i add ship type for spaceship", driver.FindElement(By.Id("ShipType")).GetAttribute("value"));
            Assert.Equal("12345", driver.FindElement(By.Id("Crew")).GetAttribute("value"));
            Assert.Equal("666667", driver.FindElement(By.Id("EnginePower")).GetAttribute("value"));

            //sisestatavad andmed
            InsertSpaceShipData(driver, true);
            IWebElement saveButton = driver.FindElement(By.CssSelector("input[type='submit'][value='Update Spaceship']"));
            saveButton.Click();

            //kontroll
            Assert.Equal("/Spaceship", new Uri(driver.Url).AbsolutePath);
            var updatedRow = FindSpaceshipRow(driver, "DISISENJUNAEM");
            //avame muutmisvormi uuesti ja kontrollime salvestatud andmeid
            IWebElement updateAgainButton = updatedRow.FindElement(By.LinkText("Update"));
            updateAgainButton.Click();
            Assert.Equal("DISISENJUNAEM", driver.FindElement(By.Id("Name")).GetAttribute("value"));
            Assert.Equal("completely unheard of haunebu", driver.FindElement(By.Id("ShipType")).GetAttribute("value"));
            Assert.Equal("67890", driver.FindElement(By.Id("Crew")).GetAttribute("value"));
            Assert.Equal("444", driver.FindElement(By.Id("EnginePower")).GetAttribute("value"));
        }
        [Fact]
        public void Should_DeleteSpaceship_FromAppViaDeleteView()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();
            var createdRow = CreateSpaceship(driver);
            //vajuta selles reas asuvat delete nuppu
            IWebElement deleteButton = createdRow.FindElement(By.LinkText("Delete"));
            deleteButton.Click();

            //kontrollime kustutatava kosmoselaeva andmeid
            Assert.StartsWith("/Spaceship/Delete/", new Uri(driver.Url).AbsolutePath);
            Assert.Equal("i add name for spaceship", driver.FindElement(By.Id("Name")).GetAttribute("value"));
            Assert.Equal("i add ship type for spaceship", driver.FindElement(By.Id("ShipType")).GetAttribute("value"));
            Assert.Equal("12345", driver.FindElement(By.Id("Crew")).GetAttribute("value"));
            Assert.Equal("666667", driver.FindElement(By.Id("EnginePower")).GetAttribute("value"));

            //kinnitame kosmoselaeva kustutamise
            IWebElement confirmDeleteButton = driver.FindElement(By.CssSelector("input[type='submit'][value='Delete Spaceship']"));
            confirmDeleteButton.Click();

            //kontrollime nimekirja naasmist ja kosmoselaeva puudumist tabelis
            Assert.Equal("/Spaceship", new Uri(driver.Url).AbsolutePath);
            Assert.True(driver.FindElement(By.CssSelector("a[href='/Spaceship/Create']")).Displayed);
            Assert.Empty(driver.FindElements(By.CssSelector("tbody tr")));
        }

        private static IWebElement CreateSpaceship(IWebDriver driver)
        {
            //tegevus
            IWebElement createInIndex = driver.FindElement(By.LinkText("Create Spaceship"));
            createInIndex.Click();
            Assert.Equal("/Spaceship/Create", new Uri(driver.Url).AbsolutePath);
            //sisestatavad andmed
            InsertSpaceShipData(driver);
            IWebElement createButton = driver.FindElement(By.CssSelector("input[type='submit'][value='Create Spaceship']"));
            createButton.Click();

            //kontroll
            Assert.Equal("/Spaceship", new Uri(driver.Url).AbsolutePath);
            //andmete kogumine pärast reloadi
            var createdRow = FindSpaceshipRow(driver, "i add name for spaceship");
            var cells = createdRow.FindElements(By.TagName("td"));
            //kontroll
            Assert.Equal("i add name for spaceship", cells[1].Text);
            Assert.Equal("i add ship type for spaceship", cells[2].Text);
            Assert.Equal("12345", cells[4].Text);
            return createdRow;
        }

        private static IWebElement FindSpaceshipRow(IWebDriver driver, string name)
        {
            //otsime nime järgi tabeli rea, rida htmli tabelis tähistatakse "tr"iga
            return driver.FindElement(By.XPath($"//tbody/tr[td[normalize-space()='{name}']]"));
        }

        private static IWebDriver CreateFirefoxDriver()
        {
            var driverService = FirefoxDriverService.CreateDefaultService(AppContext.BaseDirectory);
            var options = new FirefoxOptions
            {
                AcceptInsecureCertificates = true
            };
            //Firefox avaneb nähtava aknana, et testi klõpsamisi oleks näha.
            return new FirefoxDriver(driverService, options);
        }

        private static void InsertSpaceShipData(IWebDriver driver, bool isUpdate = false)
        {
            //sisestatavad andmed: isUpdate korral uued väärtused, muidu loomise andmed
            IWebElement cu_NameEntrySpaceship = driver.FindElement(By.Id("Name"));
            cu_NameEntrySpaceship.Clear();
            cu_NameEntrySpaceship.SendKeys(isUpdate ? "DISISENJUNAEM" : "i add name for spaceship");

            IWebElement cu_ShipTypeEntrySpaceship = driver.FindElement(By.Id("ShipType"));
            cu_ShipTypeEntrySpaceship.Clear();
            cu_ShipTypeEntrySpaceship.SendKeys(isUpdate ? "completely unheard of haunebu" : "i add ship type for spaceship");

            IWebElement cu_CrewEntrySpaceship = driver.FindElement(By.Id("Crew"));
            cu_CrewEntrySpaceship.Clear();
            cu_CrewEntrySpaceship.SendKeys(isUpdate ? "67890" : "12345");

            IWebElement cu_EnginePowerEntrySpaceship = driver.FindElement(By.Id("EnginePower"));
            cu_EnginePowerEntrySpaceship.Clear();
            cu_EnginePowerEntrySpaceship.SendKeys(isUpdate ? "444" : "666667");
        }
    }
}
