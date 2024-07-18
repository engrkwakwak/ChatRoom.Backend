using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Runtime.CompilerServices;

namespace ChatRoom.UITest.Pages
{
    public class ContactPage
    {
        private readonly IWebDriver _driver;
        private readonly string _rootUrl = Environments.FRONTEND_URI;

        private readonly WebDriverWait _wait;
        public ContactPage(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        private IWebElement UsernameInput => _driver.FindElement(By.Id("username"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("password"));
        private IWebElement SignInButton => _driver.FindElement(By.Id("signin_btn"));
        private IWebElement NbTabset => _driver.FindElement(By.TagName("nb-tabset"));
        public IWebElement ContactsTabButton => NbTabset.FindElement(By.Id("contacts_tab_title"));
        public IWebElement ContactsTab => NbTabset.FindElement(By.Id("contacts_tab"));
        private IWebElement ContactsSearchBar => NbTabset.FindElement(By.Id("contacts_searchbar"));
        private IWebElement UsersList => NbTabset.FindElement(By.Id("users_list"));
        private IWebElement ContactsList => NbTabset.FindElement(By.Id("contacts_list"));
        public IEnumerable<IWebElement> UsersListItems => UsersList.FindElements(By.ClassName("user-item"));
        public IEnumerable<IWebElement> ContactsListItems => ContactsList.FindElements(By.ClassName("user-item"));
        public IWebElement UserProfileCard => _driver.FindElement(By.Id("user_profile_card"));
        
        public string GetUserProfileName()
        {
            IWebElement displayNameLabel = UserProfileCard.FindElement(By.Id("display_name_label"));
            _wait.Until(d => displayNameLabel.Text.Length > 0);
            return displayNameLabel.Text;
        }

        public string GetUserProfileUserName()
        {
            IWebElement usernameInput = UserProfileCard.FindElement(By.Id("username"));
            _wait.Until(d => usernameInput.GetAttribute("value").Length > 0);
            return usernameInput.GetAttribute("value");
        }
        
        public string GetUserProfileEmail()
        {
            IWebElement emailInput = UserProfileCard.FindElement(By.Id("email"));
            _wait.Until(d => emailInput.Displayed);
            _wait.Until(d => emailInput.GetAttribute("value").Length > 0);
            return emailInput.GetAttribute("value");
        }

        public void AddToContacts()
        {
            IWebElement item = UsersListItems.First();
            IWebElement optionButton = item.FindElement(By.ClassName("user_option_button"));
            optionButton.Click();
            _wait.Until(d => item.FindElement(By.TagName("ul")).FindElements(By.TagName("li")).Count != 0);
            item.FindElement(By.Id("opt-add-contact")).Click();
        }

        public string GetFirstUserListItemName()
        {
            IWebElement displayName = UsersListItems.First().FindElement(By.ClassName("display_name"));
            return displayName.Text;
        }

        public string GetFirstUserListItemUsername()
        {
            IWebElement username = UsersListItems.First().FindElement(By.ClassName("username"));
            return username.Text;
        }

        public string GetFirstContactListItemName()
        {
            IWebElement displayName = ContactsListItems.First().FindElement(By.ClassName("display_name"));
            return displayName.Text;
        }

        public string GetFirstContactListItemUsername()
        {
            IWebElement username = ContactsListItems.First().FindElement(By.ClassName("username"));
            return username.Text;
        }

        public void SignIn(string username, string password)
        {
            _driver.Navigate().GoToUrl($"{_rootUrl}/#/signin");

            UsernameInput.Click();
            UsernameInput.Clear();
            UsernameInput.SendKeys(username);
            
            PasswordInput.Click();
            PasswordInput.Clear();
            PasswordInput.SendKeys(password);

            SignInButton.Click();
        }

        public void ClickContacts()
        {
            ContactsTabButton.Click();
        }

        public void Search(string keyword)
        {
            ContactsSearchBar.Click();
            ContactsSearchBar.Clear();
            ContactsSearchBar.SendKeys(keyword);
        }

        public void RemoveFromContacts()
        {
            IWebElement item = ContactsListItems.First();
            IWebElement optionButton = item.FindElement(By.ClassName("user_option_button"));
            optionButton.Click();
            _wait.Until(d => item.FindElement(By.TagName("ul")).FindElements(By.TagName("li")).Count != 0);
            item.FindElement(By.Id("opt-remove-contact")).Click();
        }

        public void ViewProfile()
        {
            IWebElement item = UsersListItems.First();
            IWebElement optionButton = item.FindElement(By.ClassName("user_option_button"));
            optionButton.Click();
            _wait.Until(d => item.FindElement(By.TagName("ul")).FindElements(By.TagName("li")).Count != 0);
            item.FindElement(By.Id("opt-view-profile")).Click();
        }
    }
}
