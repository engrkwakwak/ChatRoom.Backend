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
        public IEnumerable<IWebElement> ValidationErrors => _driver.FindElements(By.ClassName("validation-error"));
        public IWebElement InvalidAuthAlert => _driver.FindElement(By.Id("invalid-auth-alert"));

        public void NavigateToSignIn()
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

        public void FillUsernameInput(string username)
        {
            UserNameInput.Click();
            UserNameInput.Clear();
            UserNameInput.SendKeys(username);
        }

        public void FillPasswordInput(string password)
        {
            PasswordInput.Click();
            PasswordInput.Clear();
            PasswordInput.SendKeys(password);
        }
    }
}
