using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.UITest.Pages
{
    public class SignUpPage
    {
        private readonly IWebDriver _driver;
        private readonly string _rootUrl;

        public SignUpPage(IWebDriver driver)
        {
            _driver = driver;
            _rootUrl = Environments.FRONTEND_URI;
        }

        private IWebElement DisplayNameInput => _driver.FindElement(By.Id("display_name"));
        private IWebElement UserNameInput => _driver.FindElement(By.Id("username"));
        private IWebElement EmailInput => _driver.FindElement(By.Id("email"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("password"));
        private IWebElement PasswordConfirmationInput => _driver.FindElement(By.Id("password-confirmation"));
        private IWebElement SignUpButton => _driver.FindElement(By.Id("signup_btn"));

        public void Navigate()
        {
            _driver.Navigate().GoToUrl($"{_rootUrl}/#/signup");
        }

        public void SubmitSignUpForm()
        {
            SignUpButton.Click();
        }

        public void PopulateForm(string displayName, string username, string email, string password, string passwordConfirmation)
        {
            DisplayNameInput.Click();
            DisplayNameInput.Clear();
            DisplayNameInput.SendKeys(displayName);

            UserNameInput.Click();
            UserNameInput.Clear();
            UserNameInput.SendKeys(username);

            EmailInput.Click();
            EmailInput.Clear();
            EmailInput.SendKeys(email);
            
            PasswordInput.Click();
            PasswordInput.Clear();
            PasswordInput.SendKeys(password);   
            
            PasswordConfirmationInput.Click();
            PasswordConfirmationInput.Clear();
            PasswordConfirmationInput.SendKeys(passwordConfirmation);
        }
    }
}
