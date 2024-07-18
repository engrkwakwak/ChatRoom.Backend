using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ChatRoom.UITest.Pages
{
    public class UserProfilePage
    {
        private readonly IWebDriver _driver;
        private readonly string _rootUrl = Environments.FRONTEND_URI;
        private readonly WebDriverWait _wait;
        public UserProfilePage(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        private IWebElement UserProfileBtn => _driver.FindElement(By.Id("user-profile-btn"));
        private IWebElement UsernameInput => _driver.FindElement(By.Id("username"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("password"));
        private IWebElement SignInBtn => _driver.FindElement(By.Id("signin_btn"));
        public IWebElement ProfileLink => _driver.FindElement(By.LinkText("Profile"));
        public IWebElement ChangePasswordLink => _driver.FindElement(By.LinkText("Change Password"));
        public IWebElement LogoutLink => _driver.FindElement(By.LinkText("Logout"));
        public IWebElement UserProfileCard => _driver.FindElement(By.Id("user_profile_card"));
        public IWebElement NbCardBody => _driver.FindElement(By.TagName("nb-card-body"));
        public IWebElement DisplayNameLabel => UserProfileCard.FindElement(By.Id("display_name_label"));
        public IWebElement UsernameInputForUserProfile => UserProfileCard.FindElement(By.Id("username"));
        private IWebElement EmailInput => UserProfileCard.FindElement(By.Id("email"));
        private IWebElement DisplayNameInput => UserProfileCard.FindElement(By.Id("displayName"));
        private IWebElement BirthdayInput => UserProfileCard.FindElement(By.Id("birth-date"));
        private IWebElement AddressInput => UserProfileCard.FindElement(By.Id("address"));
        private IWebElement EditProfileSaveButton => UserProfileCard.FindElement(By.Id("edit_profile_save_btn"));
        public IEnumerable<IWebElement> ValidationErrors => _driver.FindElements(By.ClassName("validation-error"));
        public IWebElement ConfirmationDialog => _driver.FindElement(By.Id("confirmation-dialog"));
        private IWebElement ConfirmDiagTitle => ConfirmationDialog.FindElement(By.Id("confirm-diag-title"));
        private IWebElement ConfirmDiagMessage => ConfirmationDialog.FindElement(By.Id("confirm-diag-message"));
        public IEnumerable<IWebElement> NbToasts => _driver.FindElements(By.TagName("nb-toast"));

        public void SignIn(string username, string password)
        {
            _driver.Navigate().GoToUrl($"{_rootUrl}/#/signin");
            _wait.Until(d => d.Url == $"{_rootUrl}/#/signin");

            UsernameInput.Click();
            UsernameInput.Clear();
            UsernameInput.SendKeys(username);

            PasswordInput.Click();
            PasswordInput.Clear();
            PasswordInput.SendKeys(password);

            SignInBtn.Click();
        }

        public void ClickUserProfileButton()
        {
            UserProfileBtn.Click();
        }

        public void ViewProfile()
        {
            ProfileLink.Click();
        }

        public string GetUserProfileName()
        {
            _wait.Until(d => DisplayNameLabel.Text.Length > 0);
            return DisplayNameLabel.Text;
        }

        public string GetUserProfileUserName()
        {
            _wait.Until(d => UsernameInputForUserProfile.GetAttribute("value").Length > 0);
            return UsernameInputForUserProfile.GetAttribute("value");
        }

        public string GetUserProfileEmail()
        {
            _wait.Until(d => EmailInput.Displayed);
            _wait.Until(d => EmailInput.GetAttribute("value").Length > 0);
            return EmailInput.GetAttribute("value");
        }

        public string GetUserProfileBirthday()
        {
            _wait.Until(d => BirthdayInput.Displayed);
            _wait.Until(d => BirthdayInput.GetAttribute("value").Length > 0);
            return BirthdayInput.GetAttribute("value");
        }

        public string GetUserProfileAddress()
        {
            _wait.Until(d => AddressInput.Displayed);
            _wait.Until(d => AddressInput.GetAttribute("value").Length > 0);
            return AddressInput.GetAttribute("value");
        }

        public void EditProfile(string username, string name, string email, string birthdate, string address)
        {
            UsernameInputForUserProfile.Click();
            UsernameInputForUserProfile.Clear();
            UsernameInputForUserProfile.SendKeys(username);

            DisplayNameInput.Click();
            DisplayNameInput.Clear();
            DisplayNameInput.SendKeys(name);

            EmailInput.Click();
            EmailInput.Clear();
            EmailInput.SendKeys(email);

            BirthdayInput.Click();
            BirthdayInput.Clear();
            BirthdayInput.SendKeys(birthdate);
            DisplayNameLabel.Click();

            AddressInput.Click();
            AddressInput.Clear();
            AddressInput.SendKeys(address);

            EditProfileSaveButton.Click();
        }

        public void FillDisplayNameInput(string name)
        {
            _wait.Until(d => DisplayNameInput.GetAttribute("value").Length > 0);
            DisplayNameInput.Click();
            DisplayNameInput.Clear();
            DisplayNameInput.SendKeys(name);
            DisplayNameLabel.Click();
        }

        public void FillUsernameInput(string username)
        {
            _wait.Until(d => UsernameInputForUserProfile.GetAttribute("value").Length > 0);
            UsernameInputForUserProfile.Click();
            UsernameInputForUserProfile.Clear();
            UsernameInputForUserProfile.SendKeys(username);
            DisplayNameLabel.Click();
        }

        public void FillEmailInput(string username)
        {
            _wait.Until(d => EmailInput.GetAttribute("value").Length > 0);
            EmailInput.Click();
            EmailInput.Clear();
            EmailInput.SendKeys(username);
            DisplayNameLabel.Click();
        }

        public void FillBirthdayInput(string username)
        {
            BirthdayInput.Click();
            BirthdayInput.Clear();
            BirthdayInput.SendKeys(username);
            DisplayNameLabel.Click();
        }

        public void FillAddressInput(string username)
        {
            AddressInput.Click();
            AddressInput.Clear();
            AddressInput.SendKeys(username);
            DisplayNameLabel.Click();
        }

        public void ClickChangePasswordLink()
        {
            ChangePasswordLink.Click();
        }

        public string GetConfirmDiagTitle()
        {
            return ConfirmDiagTitle.Text;
        }

        public string GetConfirmDiagMessage()
        {
            return ConfirmDiagMessage.Text;
        }

    }
}
