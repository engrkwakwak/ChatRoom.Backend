using ChatRoom.UITest.Pages;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ChatRoom.UITest.Tests
{
    public class ContactTests : IDisposable
    {
        private readonly IWebDriver driver;

        private readonly string rootUrl = Environments.FRONTEND_URI;

        private readonly WebDriverWait wait;

        private readonly ContactPage contactPage;

        public ContactTests()
        {

            ChromeOptions options = new();
            options.AddArgument("--window-size=1920,1080");

            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20))
            {
                PollingInterval = TimeSpan.FromMilliseconds(300),
            };
            contactPage = new ContactPage(driver, wait);
        }

        public void Dispose()
        {
            driver.Dispose();
        }

        [Fact]
        public void SearchAddAndRemoveContact()
        {
            contactPage.SignIn("the_flash", "password");
            string oldUrl = driver.Url;
            wait.Until(d => d.Url != oldUrl);
            Assert.Equal($"{rootUrl}/#/chat", driver.Url);

            contactPage.ClickContacts();
            wait.Until(d => contactPage.ContactsTab.Displayed);
            Assert.True(contactPage.ContactsTab.Displayed);

            contactPage.Search("Bruce Waynes");
            wait.Until(d => contactPage.UsersListItems.Any());
            Assert.Equal("Bruce Waynes", contactPage.GetFirstUserListItemName());
            Assert.Equal("@batman", contactPage.GetFirstUserListItemUsername());

            contactPage.AddToContacts();
            wait.Until(d => contactPage.ContactsListItems.Any());
            Assert.Equal("Bruce Waynes", contactPage.GetFirstContactListItemName());
            Assert.Equal("@batman", contactPage.GetFirstContactListItemUsername());

            contactPage.RemoveFromContacts();
            wait.Until(d => !contactPage.ContactsListItems.Any());
            Assert.False(contactPage.ContactsListItems.Any());
        }

        [Fact]
        public void SearchAndViewProfile()
        {
            contactPage.SignIn("the_flash", "password");
            string oldUrl = driver.Url;
            wait.Until(d => d.Url != oldUrl);
            Assert.Equal($"{rootUrl}/#/chat", driver.Url);

            contactPage.ClickContacts();
            wait.Until(d => contactPage.ContactsTab.Displayed);
            Assert.True(contactPage.ContactsTab.Displayed);

            contactPage.Search("Bruce Waynes");
            wait.Until(d => contactPage.UsersListItems.Any());
            Assert.Equal("Bruce Waynes", contactPage.GetFirstUserListItemName());
            Assert.Equal("@batman", contactPage.GetFirstUserListItemUsername());

            contactPage.ViewProfile();
            wait.Until(d => contactPage.UserProfileCard.Displayed);
            Assert.True(contactPage.UserProfileCard.Displayed);
            Assert.Equal("Bruce Waynes", contactPage.GetUserProfileName());
            Assert.Equal("brucewaynes@test.com", contactPage.GetUserProfileEmail());
            Assert.Equal("batman", contactPage.GetUserProfileUserName());
        }
    }
}
