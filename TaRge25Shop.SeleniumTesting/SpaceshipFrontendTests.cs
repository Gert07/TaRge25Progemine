using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TaRge25Shop.SeleniumTesting
{
    //järjesti klass, mis omab omadusi mille järgi teste saab järjestada
    public class OrderAttribute : Attribute
    {
        public int Value { get; } //väärtus
        public OrderAttribute(int value) //lihtne omistuskonstruktor
        {
            Value = value;
        }
    }

    //Xunit järjesti mis implementeerib ITestCaseOrdereri liidesest "OrderTestCases" meetodi, mis on see mis testide käivitamisel
    //järjestab nad vastavalt "Order" omadusele
    public class PriorityOrderer : ITestCaseOrderer
    {
        //meetod võtab sisse kõik test cased
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            //meetod tagastab kõik test cased järjestatuna arendaja defineeritud järjesti järgi.
            return testCases.OrderBy(test =>
            {
                //leitakse omadus                                               omaduse tüüp
                var method = test.TestMethod.Method.GetCustomAttributes(typeof(OrderAttribute)).FirstOrDefault();
                //kui ei leita
                if (method == null)
                {
                    //antakse maksimaalne väärtus, ehk test teostatakse kohe, ega ei
                    //panda teiste pontensiaalselt järjekorras olevate testide vahele
                    return int.MaxValue;
                }
                //kui leitakse
                else
                {
                    //siis tagastatakse omaduse "Order" väärtus
                    return (int)method.GetConstructorArguments().Single();
                }
            }).ToList(); //pannakse loendisse, ToList on linq meetod.
        }
    }
    //enne testi klassi kirjeldatakse ära mille järgi testid sorteeritakse
    //tunnuse "TestCaseOrderer"i järgi. Sulgude vahel esimene väärtus on klassi asukoht ise, teine väärtus on nimeruum kus klass asub.
    [TestCaseOrderer("TaRge25Shop.SeleniumTesting.PriorityOrderer", "TaRge25Shop.SeleniumTesting")]
    public class SpaceshipFrontendTests
    {
        public static string thisRunGuid { get; set; } = string.Empty;


        [Fact]
        [Order(5)]
        public void Should_NavigateToCreate_AddUniqueSpaceShip_WithCorrectData_ReturnToIndex()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();

            //tegevus
            IWebElement createInIndex = driver.FindElement(By.LinkText("Create Spaceship"));
            createInIndex.Click();

            //sisestatavad andmed, siin tagastub ka kasutatud nimi
            string concreteName = InsertSpaceShipData_ReturnName(driver, false);
            thisRunGuid = concreteName;

            IWebElement cu_CreateSpaceship = driver.FindElement(By.CssSelector("input[type='submit'][value='Create Spaceship']"));
            cu_CreateSpaceship.Click();

            ////arvuti tudub
            //Thread.Sleep(1000);

            //andmete kogumine pärast reloadi
            var tableOfIndex = driver.FindElement(By.CssSelector("table.table"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            string spaceShipNameData = "";
            string spaceShipTypeData = "";
            string spaceShipCrewData = "";
            string spaceShipBuiltAtData = "";
            //IWebElement indexNameSpaceship = driver.FindElement(By.Id("IndexNameSpaceship"));
            //var spaceShipNameData = indexNameSpaceship.Text;
            //IWebElement indexTypeSpaceship = driver.FindElement(By.Id("IndexTypeSpaceship"));
            //var spaceShipTypeData = indexTypeSpaceship.Text;
            //IWebElement indexCrewSpaceship = driver.FindElement(By.Id("IndexCrewSpaceship"));
            //var spaceShipCrewData = indexCrewSpaceship.Text;
            bool doesItExist = false;
            foreach (var row in rowsOfTable)
            {
                if (row.FindElement(By.CssSelector("td:nth-child(2)")).Text == thisRunGuid)
                {
                    doesItExist = true;
                    spaceShipNameData = row.FindElement(By.CssSelector("td:nth-child(2)")).Text;
                    spaceShipTypeData = row.FindElement(By.CssSelector("td:nth-child(3)")).Text;
                    spaceShipCrewData = row.FindElement(By.CssSelector("td:nth-child(5)")).Text;
                    spaceShipBuiltAtData = row.FindElement(By.CssSelector("td:nth-child(4)")).Text;
                    break;
                }
            }
            driver.Quit();

            //kontroll
            //Assert.True(concreteName == indexNameSpaceship.Text);
            Assert.True(true == doesItExist);
            Assert.Equal(thisRunGuid, spaceShipNameData);
            Assert.True(spaceShipTypeData == "i add ship type for spaceship");
            Assert.Equal("12345", spaceShipCrewData);
            Assert.True((thisRunGuid == spaceShipNameData)
                && ("i add ship type for spaceship" == spaceShipTypeData)
                && ("12345" == spaceShipCrewData));
        }


        [Fact]
        [Order(6)]
        public void Should_NavigateToDetails_OfThisSpaceShip_WithPreviouslyCorrectData()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.CssSelector("table.table"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool isObjectFound = false;

            //tsükkel käib kõik read läbi
            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.CssSelector("td:nth-child(2)"));
                if (locatedelement.Text == thisRunGuid)
                {
                    isObjectFound = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsSpaceship"));
                    var button = rowelement.FindElement(By.LinkText("Details"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            //andmete kogumine pärast reloadi
            IWebElement details_SpaceshipId = driver.FindElement(By.CssSelector("input#Id.form-control-plaintext"));
            var details_id = details_SpaceshipId.GetAttribute("value");

            IWebElement details_SpaceshipName = driver.FindElement(By.Id("Name"));
            var details_name = details_SpaceshipName.GetAttribute("value");

            IWebElement details_SpaceshipType = driver.FindElement(By.Id("ShipType"));
            var details_type = details_SpaceshipType.GetAttribute("value");

            IWebElement details_SpaceshipCrew = driver.FindElement(By.Id("Crew"));
            var details_crew = details_SpaceshipCrew.GetAttribute("value");

            IWebElement details_SpaceshipPower = driver.FindElement(By.Id("EnginePower"));
            var details_power = details_SpaceshipPower.GetAttribute("value");

            driver.Quit();
            //kontroll
            Assert.True(isObjectFound);
            Assert.NotNull(details_id);
            // et ei oleks tühi kontrollid kõikidele andmetele
            Assert.NotNull(details_name);
            Assert.NotEmpty(details_name);
            Assert.NotNull(details_type);
            Assert.False(string.IsNullOrEmpty(details_power));
            Assert.True(!string.IsNullOrEmpty(details_crew));
            Assert.Equal(thisRunGuid, details_name);
            Assert.Equal("i add ship type for spaceship", details_type);
            Assert.Equal("12345", details_crew);
            Assert.Equal("666667", details_power);
            Assert.True(int.Parse(details_crew).GetType() == typeof(int));
            Assert.True(int.Parse(details_power).GetType() == typeof(int));
        }


        [Fact]
        [Order(7)]
        public void Should_UpdateDetails_OfThisSpaceShip_WithNewData()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.CssSelector("table.table"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool doesItExist = false;
            //tsükkel käib kõik read läbi

            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.CssSelector("td:nth-child(2)"));
                if (locatedelement.Text == thisRunGuid)
                {
                    doesItExist = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsSpaceship"));
                    var button = rowelement.FindElement(By.LinkText("Update"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            //sisestatavad andmed
            InsertSpaceShipData(driver, thisRunGuid, true);

            IWebElement cu_CreateSpaceship = driver.FindElement(By.CssSelector("input[type='submit'][value='Update Spaceship']"));
            cu_CreateSpaceship.Click();

            //andmete kogumine pärast reloadi
            var tableOfIndex = driver.FindElement(By.CssSelector("table.table"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            string spaceShipNameData = "";
            string spaceShipTypeData = "";
            string spaceShipCrewData = "";
            string spaceShipBuiltAtData = "";
            doesItExist = false;
            foreach (var row in rowsOfTable)
            {
                if (row.FindElement(By.CssSelector("td:nth-child(2)")).Text == thisRunGuid)
                {
                    doesItExist = true;
                    spaceShipNameData = row.FindElement(By.CssSelector("td:nth-child(2)")).Text;
                    spaceShipTypeData = row.FindElement(By.CssSelector("td:nth-child(3)")).Text;
                    spaceShipCrewData = row.FindElement(By.CssSelector("td:nth-child(5)")).Text;
                    spaceShipBuiltAtData = row.FindElement(By.CssSelector("td:nth-child(4)")).Text;
                    break;
                }
            }
            driver.Quit();

            ////arvuti tudub
            //Thread.Sleep(1000);
            Assert.True(doesItExist);
            Assert.NotNull(spaceShipNameData);
            Assert.Equal(thisRunGuid, spaceShipNameData);
            Assert.False(string.IsNullOrEmpty(spaceShipTypeData));
            Assert.NotEqual("i add ship type for spaceship", spaceShipTypeData);
            Assert.True(spaceShipTypeData == "completely unheard of haunebu");
            Assert.False(string.IsNullOrEmpty(spaceShipTypeData));
            //kontrolli kõik teised andmed ka üle põhjalikult.
        }


        [Fact]
        [Order(8)]
        public void Should_DeleteThisSpaceship_FromAppVIADeleteView()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateSIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.CssSelector("table.table"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool doesItExist = false;
            //tsükkel käib kõik read läbi

            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.CssSelector("td:nth-child(2)"));
                if (locatedelement.Text == thisRunGuid)
                {
                    doesItExist = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsSpaceship"));
                    var button = rowelement.FindElement(By.LinkText("Delete"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            Assert.True(doesItExist);
            var clickthisbutton = driver.FindElement(By.CssSelector("input[type='submit'][value='Delete Spaceship']"));
            clickthisbutton.Click();
            bool isDeleted = true;
            table = driver.FindElement(By.CssSelector("table.table"));
            rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            foreach (var row in rows)
            {
                var nameElement = row.FindElement(By.CssSelector("td:nth-child(2)")).Text;
                if (nameElement == thisRunGuid)
                {
                    isDeleted = false;
                    break;
                }
                else
                {
                    isDeleted = true;
                }
            }

            Assert.True(isDeleted);
            Assert.False(!isDeleted);
        }


        private static IWebDriver SetupAndNavigateSIndex()
        {
            //firefoxi käskiv ja juhtiv draiver
            IWebDriver driver = new FirefoxDriver(AppContext.BaseDirectory);
            //aadress millele draiver navigeerib
            driver.Url = "http://localhost:5000/";
            //lehelt otsitav element
            IWebElement navigateToSpaceship = driver.FindElement(By.LinkText("Spaceship"));
            //selle elemendiga tehtav tegevus
            navigateToSpaceship.Click();
            return driver;
        }


        private void InsertSpaceShipData(IWebDriver driver, string? name = null, bool isOneOrTwo = false)
        {

            if (isOneOrTwo)
            {
                IWebElement cu_NameEntrySpaceship = driver.FindElement(By.Id("Name"));
                cu_NameEntrySpaceship.Clear();
                if (name != null)
                {
                    cu_NameEntrySpaceship.SendKeys(name);
                }
                else
                {
                    cu_NameEntrySpaceship.SendKeys("DISISENJUNAEM");
                }
                IWebElement cu_ShipTypeEntrySpaceship = driver.FindElement(By.Id("ShipType"));
                cu_ShipTypeEntrySpaceship.Clear();
                cu_ShipTypeEntrySpaceship.SendKeys("completely unheard of haunebu");
                IWebElement cu_CrewEntrySpaceship = driver.FindElement(By.Id("Crew"));
                cu_CrewEntrySpaceship.Clear();
                cu_CrewEntrySpaceship.SendKeys("67890");
                IWebElement cu_EnginePowerEntrySpaceship = driver.FindElement(By.Id("EnginePower"));
                cu_EnginePowerEntrySpaceship.Clear();
                cu_EnginePowerEntrySpaceship.SendKeys("444");
            }
            else
            {
                IWebElement cu_NameEntrySpaceship = driver.FindElement(By.Id("Name"));
                cu_NameEntrySpaceship.Clear();
                if (name != null)
                {
                    cu_NameEntrySpaceship.SendKeys(name);
                }
                else
                {
                    cu_NameEntrySpaceship.SendKeys("i add name for spaceship");
                }
                IWebElement cu_ShipTypeEntrySpaceship = driver.FindElement(By.Id("ShipType"));
                cu_ShipTypeEntrySpaceship.Clear();
                cu_ShipTypeEntrySpaceship.SendKeys("i add ship type for spaceship");
                IWebElement cu_CrewEntrySpaceship = driver.FindElement(By.Id("Crew"));
                cu_CrewEntrySpaceship.Clear();
                cu_CrewEntrySpaceship.SendKeys("12345");
                IWebElement cu_EnginePowerEntrySpaceship = driver.FindElement(By.Id("EnginePower"));
                cu_EnginePowerEntrySpaceship.Clear();
                cu_EnginePowerEntrySpaceship.SendKeys("666667");
            }
        }

        private string InsertSpaceShipData_ReturnName(IWebDriver driver, bool isOneOrTwo = false)
        {
            Guid returnGuid = Guid.NewGuid();
            if (isOneOrTwo)
            {
                IWebElement cu_NameEntrySpaceship = driver.FindElement(By.Id("Name"));
                cu_NameEntrySpaceship.Clear();
                cu_NameEntrySpaceship.SendKeys(returnGuid.ToString());
                IWebElement cu_ShipTypeEntrySpaceship = driver.FindElement(By.Id("ShipType"));
                cu_ShipTypeEntrySpaceship.Clear();
                cu_ShipTypeEntrySpaceship.SendKeys("completely unheard of haunebu");
                IWebElement cu_CrewEntrySpaceship = driver.FindElement(By.Id("Crew"));
                cu_CrewEntrySpaceship.Clear();
                cu_CrewEntrySpaceship.SendKeys("67890");
                IWebElement cu_EnginePowerEntrySpaceship = driver.FindElement(By.Id("EnginePower"));
                cu_EnginePowerEntrySpaceship.Clear();
                cu_EnginePowerEntrySpaceship.SendKeys("444");
            }
            else
            {
                IWebElement cu_NameEntrySpaceship = driver.FindElement(By.Id("Name"));
                cu_NameEntrySpaceship.Clear();
                cu_NameEntrySpaceship.SendKeys(returnGuid.ToString());
                IWebElement cu_ShipTypeEntrySpaceship = driver.FindElement(By.Id("ShipType"));
                cu_ShipTypeEntrySpaceship.Clear();
                cu_ShipTypeEntrySpaceship.SendKeys("i add ship type for spaceship");
                IWebElement cu_CrewEntrySpaceship = driver.FindElement(By.Id("Crew"));
                cu_CrewEntrySpaceship.Clear();
                cu_CrewEntrySpaceship.SendKeys("12345");
                IWebElement cu_EnginePowerEntrySpaceship = driver.FindElement(By.Id("EnginePower"));
                cu_EnginePowerEntrySpaceship.Clear();
                cu_EnginePowerEntrySpaceship.SendKeys("666667");
            }
            return returnGuid.ToString();
        }
    }
}
