using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Xunit;

namespace TaRge25Shop.SeleniumTesting
{
    [TestCaseOrderer("TaRge25Shop.SeleniumTesting.PriorityOrderer", "TaRge25Shop.SeleniumTesting")]
    public class KindergardenFrontendTests
    {
        public static string thisRunGuid { get; set; } = string.Empty;

        [Fact]
        [Order(1)]
        public void Should_NavigateToCreate_AddUniqueKindergarden_WithCorrectData_ReturnToIndex()
        {
            //ülesseade
            using IWebDriver driver = SetupAndNavigateKIndex();

            //tegevus
            IWebElement createInIndex = driver.FindElement(By.LinkText("Create Kindergarden"));
            createInIndex.Click();

            thisRunGuid = Guid.NewGuid().ToString();

            IWebElement cu_GroupNameEntryKindergarden = driver.FindElement(By.Id("GroupName"));
            cu_GroupNameEntryKindergarden.Clear();
            cu_GroupNameEntryKindergarden.SendKeys(thisRunGuid);

            IWebElement cu_KindergardenNameEntryKindergarden = driver.FindElement(By.Id("KindergardenName"));
            cu_KindergardenNameEntryKindergarden.Clear();
            cu_KindergardenNameEntryKindergarden.SendKeys("Testi lasteaed");

            IWebElement cu_ChildrenCountEntryKindergarden = driver.FindElement(By.Id("ChildrenCount"));
            cu_ChildrenCountEntryKindergarden.Clear();
            cu_ChildrenCountEntryKindergarden.SendKeys("20");

            IWebElement cu_TeacherNameEntryKindergarden = driver.FindElement(By.Id("TeacherName"));
            cu_TeacherNameEntryKindergarden.Clear();
            cu_TeacherNameEntryKindergarden.SendKeys("Mari Maasikas");

            IWebElement cu_CreateKindergarden = driver.FindElement(By.CssSelector("input[type='submit'][value='Create Kindergarden']"));
            cu_CreateKindergarden.Click();

            //andmete kogumine pärast reloadi
            var tableOfIndex = driver.FindElement(By.CssSelector("table.table"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            string kindergardenGroupNameData = "";
            string kindergardenChildrenCountData = "";
            string kindergardenTeacherNameData = "";
            bool doesItExist = false;

            foreach (var row in rowsOfTable)
            {
                if (row.FindElement(By.CssSelector("td:nth-child(2)")).Text == thisRunGuid)
                {
                    doesItExist = true;
                    kindergardenGroupNameData = row.FindElement(By.CssSelector("td:nth-child(2)")).Text;
                    kindergardenChildrenCountData = row.FindElement(By.CssSelector("td:nth-child(3)")).Text;
                    kindergardenTeacherNameData = row.FindElement(By.CssSelector("td:nth-child(5)")).Text;
                    break;
                }
            }
            driver.Quit();

            //kontroll
            Assert.True(doesItExist);
            Assert.Equal(thisRunGuid, kindergardenGroupNameData);
            Assert.Equal("20", kindergardenChildrenCountData);
            Assert.Equal("Mari Maasikas", kindergardenTeacherNameData);
        }

        private static IWebDriver SetupAndNavigateKIndex()
        {
            //firefoxi käskiv ja juhtiv draiver
            IWebDriver driver = new FirefoxDriver(AppContext.BaseDirectory);
            //aadress millele draiver navigeerib
            driver.Url = "http://localhost:5000/";
            //lehelt otsitav element
            IWebElement navigateToKindergarden = driver.FindElement(By.LinkText("Kindergarden"));
            navigateToKindergarden.Click();
            return driver;
        }
    }
}
