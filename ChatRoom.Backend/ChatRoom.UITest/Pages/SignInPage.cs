using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.UITest.Pages
{
    public class SignInPage
    {
        private readonly IWebDriver _driver;
        private readonly string _rootUrl = Environments.FRONTEND_URI;
        public SignInPage(IWebDriver driver) 
        {
            _driver = driver;
        }

        private IWebElement UserNameInput => _driver.FindElement(By.Id("username"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("password"));
        private IWebElement SignInButton => _driver.FindElement(By.Id("signin_btn"));

        public void Navigate()
        {
            _driver.Navigate().GoToUrl($"{_rootUrl}/#/signin");
        }

        public void SubmitSignInForm()
        {
            SignInButton.Click();
        }

        public void PopulateForm(string username, string password)
        {

            UserNameInput.Click();
            UserNameInput.Clear();
            UserNameInput.SendKeys(username);

            PasswordInput.Click();
            PasswordInput.Clear();
            PasswordInput.SendKeys(password);
        }
    }
}
