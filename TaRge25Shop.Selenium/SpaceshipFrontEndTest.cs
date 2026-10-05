using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TaRge25Shop.Selenium
{
    public class SpaceshipFrontEndTest
    {
        [Fact]
        public void Should_NavigateToCreate_AddSpaceship_WithCorrectData()
        {
            //firefoxi käskiv ja juhtiv draiver
            IWebDriver driver = new FirefoxDriver();
            //aadress millele draiver navigeerib
            driver.Url = "https://localhost:7128/";
            //lehelt otsitav element
            IWebElement navigateToSpaceship = driver.FindElement(By.LinkText("Spaceship"));
            //tegevus selle elemendiga
            navigateToSpaceship.Click();
            IWebElement createInIndex = driver.FindElement(By.Id("CreateInIndex"));
            navigateToSpaceship.Click();
        }
    }
}
