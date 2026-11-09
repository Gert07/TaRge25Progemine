using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using Xunit;

namespace TaRge25Shop.SeleniumTesting
{
    public class KindergardenFrontendTests
    {
        public static string thisRunGuid { get; set; }


        [Fact]
        [Order(1)]
        public void Should_NavigateToCreate_AddUniqueKindergarden_WithCorrectData_ReturnToIndex()
        {
            //ülesseade
            IWebDriver driver = SetupAndNavigateKIndex();

            //tegevus
            IWebElement createInIndex = driver.FindElement(By.Id("CreateInIndex"));
            createInIndex.Click();

            //sisestatavad andmed, siin tagastub ka kasutatud nimi
            string concreteName = InsertKindergardenData_ReturnName(driver, false);
            thisRunGuid = concreteName;

            IWebElement cu_CreateKindergarden = driver.FindElement(By.Id("CU_CreateKindergarden"));
            cu_CreateKindergarden.Click();

            ////arvuti tudub
            //Thread.Sleep(1000);

            //andmete kogumine pärast reloadi
            var tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            string kindergardenGroupNameData = "";
            string kindergardenTeacherNameData = "";
            string kindergardenChildrenCountData = "";
            string kindergardenCreatedAtData = "";
            bool doesItExist = false;
            foreach (var row in rowsOfTable)
            {
                if (row.FindElement(By.Id("IndexGroupNameKindergarden")).Text == thisRunGuid)
                {
                    doesItExist = true;
                    kindergardenGroupNameData = thisRunGuid;
                    kindergardenTeacherNameData = row.FindElement(By.Id("IndexTeacherNameKindergarden")).Text;
                    kindergardenChildrenCountData = row.FindElement(By.Id("IndexChildrenCountKindergarden")).Text;
                    kindergardenCreatedAtData = row.FindElement(By.Id("IndexCreatedDateKindergarden")).Text;
                    break;
                }
            }
            driver.Quit();

            //kontroll
            //Assert.True(concreteName == indexGroupNameKindergarden.Text);
            Assert.True(true == doesItExist);
            Assert.Equal(kindergardenGroupNameData, thisRunGuid);
            Assert.True(kindergardenTeacherNameData == "i add teacher name for kindergarden");
            Assert.Equal(kindergardenChildrenCountData, "12345");
            Assert.True((thisRunGuid == kindergardenGroupNameData)
                && ("i add teacher name for kindergarden" == kindergardenTeacherNameData)
                && ("12345" == kindergardenChildrenCountData));
        }


        [Fact]
        [Order(2)]
        public void Should_NavigateToDetails_OfThisKindergarden_WithPreviouslyCorrectData()
        {
            //ülesseade
            IWebDriver driver = SetupAndNavigateKIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool isObjectFound = false;

            //tsükkel käib kõik read läbi
            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.Id("IndexGroupNameKindergarden"));
                if (locatedelement.Text == thisRunGuid)
                {
                    isObjectFound = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsKindergarden"));
                    var button = rowelement.FindElement(By.Id("IndexKindergardenDetails"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            //andmete kogumine pärast reloadi
            IWebElement details_KindergardenId = driver.FindElement(By.Id("Details_KindergardenId"));
            var details_id = details_KindergardenId.Text;

            IWebElement details_KindergardenGroupName = driver.FindElement(By.Id("Details_KindergardenGroupName"));
            var details_groupName = details_KindergardenGroupName.Text;

            IWebElement details_KindergardenTeacherName = driver.FindElement(By.Id("Details_KindergardenTeacherName"));
            var details_teacherName = details_KindergardenTeacherName.Text;

            IWebElement details_KindergardenChildrenCount = driver.FindElement(By.Id("Details_KindergardenChildrenCount"));
            var details_childrenCount = details_KindergardenChildrenCount.Text;

            IWebElement details_KindergardenName = driver.FindElement(By.Id("Details_KindergardenName"));
            var details_kindergardenName = details_KindergardenName.Text;

            driver.Quit();
            //kontroll
            Assert.True(isObjectFound);
            Assert.NotNull(details_id);
            // et ei oleks tühi kontrollid kõikidele andmetele
            Assert.NotEmpty(details_groupName);
            Assert.NotNull(details_teacherName);
            Assert.False(string.IsNullOrEmpty(details_kindergardenName));
            Assert.True(!string.IsNullOrEmpty(details_childrenCount));
            Assert.Equal(thisRunGuid, details_groupName);
            Assert.Equal("i add teacher name for kindergarden", details_teacherName);
            Assert.Equal("12345", details_childrenCount);
            Assert.Equal("666667", details_kindergardenName);
            Assert.True(int.Parse(details_childrenCount).GetType() == typeof(int));
            Assert.True(int.Parse(details_kindergardenName).GetType() == typeof(int));
        }


        [Fact]
        [Order(3)]
        public void Should_UpdateDetails_OfThisKindergarden_WithNewData()
        {
            //ülesseade
            IWebDriver driver = SetupAndNavigateKIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool doesItExist = false;
            //tsükkel käib kõik read läbi

            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.Id("IndexGroupNameKindergarden"));
                if (locatedelement.Text == thisRunGuid)
                {
                    doesItExist = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsKindergarden"));
                    var button = rowelement.FindElement(By.Id("IndexKindergardenUpdate"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            //sisestatavad andmed
            InsertKindergardenData(driver, thisRunGuid, true);

            IWebElement cu_CreateKindergarden = driver.FindElement(By.Id("CU_UpdateKindergarden"));
            cu_CreateKindergarden.Click();

            //andmete kogumine pärast reloadi
            var tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            string kindergardenGroupNameData = "";
            string kindergardenTeacherNameData = "";
            string kindergardenChildrenCountData = "";
            string kindergardenCreatedAtData = "";
            doesItExist = false;
            foreach (var row in rowsOfTable)
            {
                if (row.FindElement(By.Id("IndexGroupNameKindergarden")).Text == thisRunGuid)
                {
                    doesItExist = true;
                    kindergardenGroupNameData = thisRunGuid;
                    kindergardenTeacherNameData = row.FindElement(By.Id("IndexTeacherNameKindergarden")).Text;
                    kindergardenChildrenCountData = row.FindElement(By.Id("IndexChildrenCountKindergarden")).Text;
                    kindergardenCreatedAtData = row.FindElement(By.Id("IndexCreatedDateKindergarden")).Text;
                    break;
                }
            }
            driver.Quit();

            ////arvuti tudub
            //Thread.Sleep(1000);
            Assert.True(doesItExist);
            Assert.NotNull(kindergardenGroupNameData);
            Assert.Equal(thisRunGuid, kindergardenGroupNameData);
            Assert.False(string.IsNullOrEmpty(kindergardenTeacherNameData));
            Assert.NotEqual(kindergardenTeacherNameData, "i add teacher name for kindergarden");
            Assert.True(kindergardenTeacherNameData == "completely unheard of haunebu");
            Assert.False(string.IsNullOrEmpty(kindergardenTeacherNameData));
            //kontrolli kõik teised andmed ka üle põhjalikult.
        }


        [Fact]
        [Order(4)]
        public void Should_DeleteThisKindergarden_FromAppVIADeleteView()
        {
            //ülesseade
            IWebDriver driver = SetupAndNavigateKIndex();

            //otsime üles kõik tabeli read, rida htmli tabelis tähistatakse "tr"iga
            //ICollection<IWebElement> table = driver.FindElements(By.TagName("tr"));
            var table = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            bool doesItExist = false;
            //tsükkel käib kõik read läbi

            foreach (var row in rows)
            {

                var locatedelement = row.FindElement(By.Id("IndexGroupNameKindergarden"));
                if (locatedelement.Text == thisRunGuid)
                {
                    doesItExist = true;
                    //siis vajuta selles reas asuvat details nuppu
                    var rowelement = row.FindElement(By.Id("IndexActionsKindergarden"));
                    var button = rowelement.FindElement(By.Id("IndexKindergardenDelete"));
                    button.Click();
                    //pärast õiget vajutust, tsükkel katkestatakse
                    //arvuti tudub
                    Thread.Sleep(500);
                    break;
                }
            }
            var clickthisbutton = driver.FindElement(By.Id("Delete_Delete"));
            clickthisbutton.Click();
            bool isDeleted = false;
            table = driver.FindElement(By.Id("IndexTable"));
            rows = table.FindElements(By.TagName("tr")).ToList();
            rows.RemoveAt(0);
            foreach (var row in rows)
            {
                var nameElement = row.FindElement(By.Id("IndexGroupNameKindergarden")).Text;
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


        private static IWebDriver SetupAndNavigateKIndex()
        {
            //firefoxi käskiv ja juhtiv draiver
            IWebDriver driver = new FirefoxDriver();
            //aadress millele draiver navigeerib
            driver.Url = "http://localhost:5000/";
            //lehelt otsitav element
            IWebElement navigateToKindergarden = driver.FindElement(By.LinkText("Kindergarden"));
            //selle elemendiga tehtav tegevus
            navigateToKindergarden.Click();
            return driver;
        }


        private void InsertKindergardenData(IWebDriver driver, string name = null, bool isOneOrTwo = false)
        {

            if (isOneOrTwo)
            {
                IWebElement cu_GroupNameEntryKindergarden = driver.FindElement(By.Id("CU_GroupNameEntryKindergarden"));
                cu_GroupNameEntryKindergarden.Clear();
                if (name != null)
                {
                    cu_GroupNameEntryKindergarden.SendKeys(name);
                }
                else
                {
                    cu_GroupNameEntryKindergarden.SendKeys("DISISENJUNAEM");
                }
                IWebElement cu_TeacherNameEntryKindergarden = driver.FindElement(By.Id("CU_TeacherNameEntryKindergarden"));
                cu_TeacherNameEntryKindergarden.Clear();
                cu_TeacherNameEntryKindergarden.SendKeys("completely unheard of haunebu");
                IWebElement cu_ChildrenCountEntryKindergarden = driver.FindElement(By.Id("CU_ChildrenCountEntryKindergarden"));
                cu_ChildrenCountEntryKindergarden.Clear();
                cu_ChildrenCountEntryKindergarden.SendKeys("67890");
                IWebElement cu_KindergardenNameEntryKindergarden = driver.FindElement(By.Id("CU_KindergardenNameEntryKindergarden"));
                cu_KindergardenNameEntryKindergarden.Clear();
                cu_KindergardenNameEntryKindergarden.SendKeys("444");
            }
            else
            {
                IWebElement cu_GroupNameEntryKindergarden = driver.FindElement(By.Id("CU_GroupNameEntryKindergarden"));
                cu_GroupNameEntryKindergarden.Clear();
                if (name != null)
                {
                    cu_GroupNameEntryKindergarden.SendKeys(name);
                }
                else
                {
                    cu_GroupNameEntryKindergarden.SendKeys("i add group name for kindergarden");
                }
                IWebElement cu_TeacherNameEntryKindergarden = driver.FindElement(By.Id("CU_TeacherNameEntryKindergarden"));
                cu_TeacherNameEntryKindergarden.Clear();
                cu_TeacherNameEntryKindergarden.SendKeys("i add teacher name for kindergarden");
                IWebElement cu_ChildrenCountEntryKindergarden = driver.FindElement(By.Id("CU_ChildrenCountEntryKindergarden"));
                cu_ChildrenCountEntryKindergarden.Clear();
                cu_ChildrenCountEntryKindergarden.SendKeys("12345");
                IWebElement cu_KindergardenNameEntryKindergarden = driver.FindElement(By.Id("CU_KindergardenNameEntryKindergarden"));
                cu_KindergardenNameEntryKindergarden.Clear();
                cu_KindergardenNameEntryKindergarden.SendKeys("666667");
            }
        }

        private string InsertKindergardenData_ReturnName(IWebDriver driver, bool isOneOrTwo = false)
        {
            Guid returnGuid = Guid.NewGuid();
            if (isOneOrTwo)
            {
                IWebElement cu_GroupNameEntryKindergarden = driver.FindElement(By.Id("CU_GroupNameEntryKindergarden"));
                cu_GroupNameEntryKindergarden.Clear();
                cu_GroupNameEntryKindergarden.SendKeys(returnGuid.ToString());
                IWebElement cu_TeacherNameEntryKindergarden = driver.FindElement(By.Id("CU_TeacherNameEntryKindergarden"));
                cu_TeacherNameEntryKindergarden.Clear();
                cu_TeacherNameEntryKindergarden.SendKeys("completely unheard of haunebu");
                IWebElement cu_ChildrenCountEntryKindergarden = driver.FindElement(By.Id("CU_ChildrenCountEntryKindergarden"));
                cu_ChildrenCountEntryKindergarden.Clear();
                cu_ChildrenCountEntryKindergarden.SendKeys("67890");
                IWebElement cu_KindergardenNameEntryKindergarden = driver.FindElement(By.Id("CU_KindergardenNameEntryKindergarden"));
                cu_KindergardenNameEntryKindergarden.Clear();
                cu_KindergardenNameEntryKindergarden.SendKeys("444");
            }
            else
            {
                IWebElement cu_GroupNameEntryKindergarden = driver.FindElement(By.Id("CU_GroupNameEntryKindergarden"));
                cu_GroupNameEntryKindergarden.Clear();
                cu_GroupNameEntryKindergarden.SendKeys(returnGuid.ToString());
                IWebElement cu_TeacherNameEntryKindergarden = driver.FindElement(By.Id("CU_TeacherNameEntryKindergarden"));
                cu_TeacherNameEntryKindergarden.Clear();
                cu_TeacherNameEntryKindergarden.SendKeys("i add teacher name for kindergarden");
                IWebElement cu_ChildrenCountEntryKindergarden = driver.FindElement(By.Id("CU_ChildrenCountEntryKindergarden"));
                cu_ChildrenCountEntryKindergarden.Clear();
                cu_ChildrenCountEntryKindergarden.SendKeys("12345");
                IWebElement cu_KindergardenNameEntryKindergarden = driver.FindElement(By.Id("CU_KindergardenNameEntryKindergarden"));
                cu_KindergardenNameEntryKindergarden.Clear();
                cu_KindergardenNameEntryKindergarden.SendKeys("666667");
            }
            return returnGuid.ToString();
        }
    }
}

