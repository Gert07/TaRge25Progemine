using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using Xunit;

namespace TaRge25Shop.SeleniumTesting
{
    public class RealEstateFrontendTests
    {
        public static string thisRunGuid { get; set; } = string.Empty;


        [Fact]
        [Order(9)]
        public void Should_NavigateToCreate_AddUniqueRealEstate_WithCorrectData_ReturnToIndex()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateREIndex();

            //tegevus
            IWebElement createInIndex = driver.FindElement(By.Id("CreateInIndex"));
            createInIndex.Click();

            //sisestatavad andmed, siin tagastub ka kasutatud nimi
            string concreteName = InsertRealEstateData_ReturnName(driver, false);
            thisRunGuid = concreteName;

            IWebElement cu_CreateRealEstate = driver.FindElement(By.Id("CU_CreateRealEstate"));
            cu_CreateRealEstate.Click();

            ////arvuti tudub
            //Thread.Sleep(1000);

            //andmete kogumine pärast reloadi
            var tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            string realEstateLocationData = "";
            string realEstateBuildingTypeData = "";
            string realEstateAreaData = "";
            string realEstateCreatedAtData = "";
            //IWebElement indexLocationRealEstate = driver.FindElement(By.Id("IndexLocationRealEstate"));
            //var realEstateLocationData = indexLocationRealEstate.Text;
            //IWebElement indexBuildingTypeRealEstate = driver.FindElement(By.Id("IndexBuildingTypeRealEstate"));
            //var realEstateBuildingTypeData = indexBuildingTypeRealEstate.Text;
            //IWebElement indexAreaRealEstate = driver.FindElement(By.Id("IndexAreaRealEstate"));
            //var realEstateAreaData = indexAreaRealEstate.Text;
            bool doesItExist = false;
            foreach (var row in rowsOfTable)
            {
                if (row.FindElement(By.Id("IndexLocationRealEstate")).Text == thisRunGuid)
                {
                    doesItExist = true;
                    realEstateLocationData = thisRunGuid;
                    realEstateBuildingTypeData = row.FindElement(By.Id("IndexBuildingTypeRealEstate")).Text;
                    realEstateAreaData = row.FindElement(By.Id("IndexAreaRealEstate")).Text;
                    realEstateCreatedAtData = row.FindElement(By.Id("IndexCreatedDateRealEstate")).Text;
                    break;
                }
            }
            driver.Quit();

            //kontroll
            //Assert.True(concreteName == indexLocationRealEstate.Text);
            Assert.True(true == doesItExist);
            Assert.Equal(thisRunGuid, realEstateLocationData);
            Assert.True(realEstateBuildingTypeData == "Apartment");
            Assert.Equal("120", realEstateAreaData);
            Assert.True((thisRunGuid == realEstateLocationData)
                && ("Apartment" == realEstateBuildingTypeData)
                && ("120" == realEstateAreaData));
        }


        [Fact]
        [Order(10)]
        public void Should_NavigateToDetails_OfThisRealEstate_WithPreviouslyCorrectData()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateREIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool isObjectFound = false;

            //tsükkel käib kõik read läbi
            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.Id("IndexLocationRealEstate"));
                if (locatedelement.Text == thisRunGuid)
                {
                    isObjectFound = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsRealEstate"));
                    var button = rowelement.FindElement(By.Id("IndexRealEstateDetails"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            //andmete kogumine pärast reloadi
            IWebElement details_RealEstateId = driver.FindElement(By.Id("Details_RealEstateId"));
            var details_id = details_RealEstateId.GetDomProperty("value");

            IWebElement details_RealEstateLocation = driver.FindElement(By.Id("Details_RealEstateLocation"));
            var details_location = details_RealEstateLocation.GetDomProperty("value");

            IWebElement details_RealEstateBuildingType = driver.FindElement(By.Id("Details_RealEstateBuildingType"));
            var details_buildingType = details_RealEstateBuildingType.GetDomProperty("value");

            IWebElement details_RealEstateArea = driver.FindElement(By.Id("Details_RealEstateArea"));
            var details_area = details_RealEstateArea.GetDomProperty("value");

            IWebElement details_RealEstateRoomNumber = driver.FindElement(By.Id("Details_RealEstateRoomNumber"));
            var details_roomNumber = details_RealEstateRoomNumber.GetDomProperty("value");

            driver.Quit();
            //kontroll
            Assert.True(isObjectFound);
            Assert.NotNull(details_id);
            // et ei oleks tühi kontrollid kõikidele andmetele
            Assert.False(string.IsNullOrEmpty(details_location));
            Assert.NotNull(details_buildingType);
            Assert.False(string.IsNullOrEmpty(details_roomNumber));
            Assert.True(!string.IsNullOrEmpty(details_area));
            Assert.Equal(thisRunGuid, details_location);
            Assert.Equal("Apartment", details_buildingType);
            Assert.Equal("120", details_area);
            Assert.Equal("3", details_roomNumber);
            Assert.True(double.TryParse(details_area, out _));
            Assert.True(int.TryParse(details_roomNumber, out _));
        }


        [Fact]
        [Order(11)]
        public void Should_UpdateDetails_OfThisRealEstate_WithNewData()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateREIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool doesItExist = false;
            //tsükkel käib kõik read läbi

            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.Id("IndexLocationRealEstate"));
                if (locatedelement.Text == thisRunGuid)
                {
                    doesItExist = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsRealEstate"));
                    var button = rowelement.FindElement(By.Id("IndexRealEstateUpdate"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            //sisestatavad andmed
            InsertRealEstateData(driver, thisRunGuid, true);

            IWebElement cu_CreateRealEstate = driver.FindElement(By.Id("CU_UpdateRealEstate"));
            cu_CreateRealEstate.Click();

            //andmete kogumine pärast reloadi
            var tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            string realEstateLocationData = "";
            string realEstateBuildingTypeData = "";
            string realEstateAreaData = "";
            string realEstateCreatedAtData = "";
            doesItExist = false;
            foreach (var row in rowsOfTable)
            {
                if (row.FindElement(By.Id("IndexLocationRealEstate")).Text == thisRunGuid)
                {
                    doesItExist = true;
                    realEstateLocationData = thisRunGuid;
                    realEstateBuildingTypeData = row.FindElement(By.Id("IndexBuildingTypeRealEstate")).Text;
                    realEstateAreaData = row.FindElement(By.Id("IndexAreaRealEstate")).Text;
                    realEstateCreatedAtData = row.FindElement(By.Id("IndexCreatedDateRealEstate")).Text;
                    break;
                }
            }
            driver.Quit();

            ////arvuti tudub
            //Thread.Sleep(1000);
            Assert.True(doesItExist);
            Assert.NotNull(realEstateLocationData);
            Assert.Equal(thisRunGuid, realEstateLocationData);
            Assert.False(string.IsNullOrEmpty(realEstateBuildingTypeData));
            Assert.NotEqual("Apartment", realEstateBuildingTypeData);
            Assert.True(realEstateBuildingTypeData == "Townhouse");
            Assert.False(string.IsNullOrEmpty(realEstateBuildingTypeData));
            //kontrolli kõik teised andmed ka üle põhjalikult.
        }


        [Fact]
        [Order(12)]
        public void Should_DeleteThisRealEstate_FromAppVIADeleteView()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateREIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool doesItExist = false;
            //tsükkel käib kõik read läbi

            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.Id("IndexLocationRealEstate"));
                if (locatedelement.Text == thisRunGuid)
                {
                    doesItExist = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsRealEstate"));
                    var button = rowelement.FindElement(By.Id("IndexRealEstateDelete"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            Assert.True(doesItExist);
            var clickthisbutton = driver.FindElement(By.Id("Delete_Delete"));
            clickthisbutton.Click();
            bool isDeleted = true;
            table = driver.FindElement(By.Id("IndexTable"));
            rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            foreach (var row in rows)
            {
                var nameElement = row.FindElement(By.Id("IndexLocationRealEstate")).Text;
                if (nameElement == thisRunGuid)
                {
                    isDeleted = false;
                    break;
                }
            }

            Assert.True(isDeleted);
        }


        private static IWebDriver SetupAndNavigateREIndex()
        {
            //firefoxi käskiv ja juhtiv draiver
            IWebDriver driver = new FirefoxDriver();
            //aadress millele draiver navigeerib
            driver.Url = "http://localhost:5000/";
            //lehelt otsitav element
            IWebElement navigateToRealEstate = driver.FindElement(By.LinkText("RealEstate"));
            //selle elemendiga tehtav tegevus
            navigateToRealEstate.Click();
            return driver;
        }


        private void InsertRealEstateData(IWebDriver driver, string? name = null, bool isOneOrTwo = false)
        {

            if (isOneOrTwo)
            {
                IWebElement cu_LocationEntryRealEstate = driver.FindElement(By.Id("CU_LocationEntryRealEstate"));
                cu_LocationEntryRealEstate.Clear();
                if (name != null)
                {
                    cu_LocationEntryRealEstate.SendKeys(name);
                }
                else
                {
                    cu_LocationEntryRealEstate.SendKeys("Updated test property");
                }
                IWebElement cu_BuildingTypeEntryRealEstate = driver.FindElement(By.Id("CU_BuildingTypeEntryRealEstate"));
                cu_BuildingTypeEntryRealEstate.Clear();
                cu_BuildingTypeEntryRealEstate.SendKeys("Townhouse");
                IWebElement cu_AreaEntryRealEstate = driver.FindElement(By.Id("CU_AreaEntryRealEstate"));
                cu_AreaEntryRealEstate.Clear();
                cu_AreaEntryRealEstate.SendKeys("145");
                IWebElement cu_RoomNumberEntryRealEstate = driver.FindElement(By.Id("CU_RoomNumberEntryRealEstate"));
                cu_RoomNumberEntryRealEstate.Clear();
                cu_RoomNumberEntryRealEstate.SendKeys("4");
            }
            else
            {
                IWebElement cu_LocationEntryRealEstate = driver.FindElement(By.Id("CU_LocationEntryRealEstate"));
                cu_LocationEntryRealEstate.Clear();
                if (name != null)
                {
                    cu_LocationEntryRealEstate.SendKeys(name);
                }
                else
                {
                    cu_LocationEntryRealEstate.SendKeys("Test property");
                }
                IWebElement cu_BuildingTypeEntryRealEstate = driver.FindElement(By.Id("CU_BuildingTypeEntryRealEstate"));
                cu_BuildingTypeEntryRealEstate.Clear();
                cu_BuildingTypeEntryRealEstate.SendKeys("Apartment");
                IWebElement cu_AreaEntryRealEstate = driver.FindElement(By.Id("CU_AreaEntryRealEstate"));
                cu_AreaEntryRealEstate.Clear();
                cu_AreaEntryRealEstate.SendKeys("120");
                IWebElement cu_RoomNumberEntryRealEstate = driver.FindElement(By.Id("CU_RoomNumberEntryRealEstate"));
                cu_RoomNumberEntryRealEstate.Clear();
                cu_RoomNumberEntryRealEstate.SendKeys("3");
            }
        }

        private string InsertRealEstateData_ReturnName(IWebDriver driver, bool isOneOrTwo = false)
        {
            Guid returnGuid = Guid.NewGuid();
            if (isOneOrTwo)
            {
                IWebElement cu_LocationEntryRealEstate = driver.FindElement(By.Id("CU_LocationEntryRealEstate"));
                cu_LocationEntryRealEstate.Clear();
                cu_LocationEntryRealEstate.SendKeys(returnGuid.ToString());
                IWebElement cu_BuildingTypeEntryRealEstate = driver.FindElement(By.Id("CU_BuildingTypeEntryRealEstate"));
                cu_BuildingTypeEntryRealEstate.Clear();
                cu_BuildingTypeEntryRealEstate.SendKeys("Townhouse");
                IWebElement cu_AreaEntryRealEstate = driver.FindElement(By.Id("CU_AreaEntryRealEstate"));
                cu_AreaEntryRealEstate.Clear();
                cu_AreaEntryRealEstate.SendKeys("145");
                IWebElement cu_RoomNumberEntryRealEstate = driver.FindElement(By.Id("CU_RoomNumberEntryRealEstate"));
                cu_RoomNumberEntryRealEstate.Clear();
                cu_RoomNumberEntryRealEstate.SendKeys("4");
            }
            else
            {
                IWebElement cu_LocationEntryRealEstate = driver.FindElement(By.Id("CU_LocationEntryRealEstate"));
                cu_LocationEntryRealEstate.Clear();
                cu_LocationEntryRealEstate.SendKeys(returnGuid.ToString());
                IWebElement cu_BuildingTypeEntryRealEstate = driver.FindElement(By.Id("CU_BuildingTypeEntryRealEstate"));
                cu_BuildingTypeEntryRealEstate.Clear();
                cu_BuildingTypeEntryRealEstate.SendKeys("Apartment");
                IWebElement cu_AreaEntryRealEstate = driver.FindElement(By.Id("CU_AreaEntryRealEstate"));
                cu_AreaEntryRealEstate.Clear();
                cu_AreaEntryRealEstate.SendKeys("120");
                IWebElement cu_RoomNumberEntryRealEstate = driver.FindElement(By.Id("CU_RoomNumberEntryRealEstate"));
                cu_RoomNumberEntryRealEstate.Clear();
                cu_RoomNumberEntryRealEstate.SendKeys("3");
            }
            return returnGuid.ToString();
        }
    }
}

