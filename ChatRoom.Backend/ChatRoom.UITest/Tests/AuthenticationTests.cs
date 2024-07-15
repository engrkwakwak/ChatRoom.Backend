using ChatRoom.UITest.Pages;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace ChatRoom.UITest.Tests
{
    public class AuthenticationTests : IDisposable
    {
        private readonly IWebDriver driver;

        private readonly string rootUrl = Environments.FRONTEND_URI;

        private readonly WebDriverWait wait;

        private readonly SignInPage signInPage;

        private readonly SignUpPage signUpPage;

        public AuthenticationTests()
        {
            ChromeOptions options = new();
            options.AddArgument("--window-size=1920,1080");

            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            signInPage = new SignInPage(driver);
            signUpPage = new SignUpPage(driver);
        }

        public void Dispose()
        {
            driver.Dispose();
        }

        [Fact]
        public async Task SignUp()
        {
            signUpPage.Navigate();
            signUpPage.PopulateForm("Test User", "testuser", "test@email.com", "password", "password");
            signUpPage.SubmitSignUpForm();
            string oldUrl = driver.Url;
            wait.Until(d => d.Url != oldUrl);

            Assert.Matches($"{rootUrl}/#/signin", driver.Url);
        }

        [Fact]
        public async Task SignIn()
        {
            signInPage.Navigate();
            signInPage.PopulateForm("testuser", "password");
            signInPage.SubmitSignInForm();
            string oldUrl = driver.Url;
            wait.Until(d => d.Url != oldUrl);

            Assert.Equal($"{rootUrl}/#/email-verification", driver.Url);
        }

        

        
    }
}
