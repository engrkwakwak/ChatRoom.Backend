using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using ChatRoom.UITest.Pages;
using OpenQA.Selenium.Chrome;

namespace ChatRoom.UITest.Tests
{
    public class UserProfileTests : IDisposable
    {
        private readonly IWebDriver driver;

        private readonly string rootUrl = Environments.FRONTEND_URI;

        private readonly WebDriverWait wait;

        private readonly UserProfilePage userProfilePage;
        public UserProfileTests()
        {
            ChromeOptions options = new();
            options.AddArgument("--window-size=1920,1080");

            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            userProfilePage = new UserProfilePage(driver, wait);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
        }

        public void Dispose()
        {
            driver.Dispose();
        }

        [Fact]
        public void ViewAndEditProfile_WithValidData()
        {
            userProfilePage.SignIn("the_flash", "password");
            wait.Until(d => d.Url == $"{rootUrl}/#/chat");
            Assert.Equal($"{rootUrl}/#/chat", driver.Url);

            userProfilePage.ClickUserProfileButton();
            wait.Until(d => userProfilePage.ProfileLink.Displayed);
            wait.Until(d => userProfilePage.ChangePasswordLink.Displayed);
            wait.Until(d => userProfilePage.LogoutLink.Displayed);
            Assert.True(userProfilePage.LogoutLink.Displayed );
            Assert.True(userProfilePage.ChangePasswordLink.Displayed);
            Assert.True(userProfilePage.LogoutLink.Displayed);

            userProfilePage.ViewProfile();
            wait.Until(d => userProfilePage.UserProfileCard.Displayed);
            Assert.True(userProfilePage.UserProfileCard.Displayed);
            Assert.Equal("Barry Allen", userProfilePage.GetUserProfileName());
            Assert.Equal("the_flash", userProfilePage.GetUserProfileUserName());
            Assert.Equal("barryallen@test.com", userProfilePage.GetUserProfileEmail());

            userProfilePage.EditProfile("the_flash", "Barry Allen", "barryallen@test.com", "Jul 7, 2024", "Makati Metro Manila");
            userProfilePage.ClickUserProfileButton();
            userProfilePage.ViewProfile();
            wait.Until(d => userProfilePage.UserProfileCard.Displayed);
            Assert.Equal("Barry Allen", userProfilePage.GetUserProfileName());
            Assert.Equal("the_flash", userProfilePage.GetUserProfileUserName());
            Assert.Equal("barryallen@test.com", userProfilePage.GetUserProfileEmail());
        }

        [Fact]
        public void ViewAndEditProfile_WithInvalidData()
        {
            userProfilePage.SignIn("the_flash", "password");
            wait.Until(d => d.Url == $"{rootUrl}/#/chat");
            Assert.Equal($"{rootUrl}/#/chat", driver.Url);

            userProfilePage.ClickUserProfileButton();
            wait.Until(d => userProfilePage.ProfileLink.Displayed);

            userProfilePage.ViewProfile();
            wait.Until(d => userProfilePage.UserProfileCard.Displayed);

            userProfilePage.FillUsernameInput("1234567890 1234567890 1234567890");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Max characters for Username is 20.") == 1);

            userProfilePage.FillUsernameInput("batman");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Username is already taken.") == 1);

            userProfilePage.FillUsernameInput("bat man");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Username should not include spaces.") == 1);

            userProfilePage.FillDisplayNameInput(" ");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Display name is required.") == 1);

            userProfilePage.FillDisplayNameInput("1234567890 1234567890 1234567890 1234567890 1234567890 ");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Max characters for Display name is 20.") == 1);

            userProfilePage.FillEmailInput("0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 ");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Max characters for Email is 100.") == 1);

            userProfilePage.FillEmailInput("INVALIDEMAIL");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Invalid email address.") == 1);

            userProfilePage.FillEmailInput("brucewaynes@test.com");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Email address is already taken.") == 1);

            userProfilePage.FillBirthdayInput(DateTime.Today.AddDays(1).ToString("MMM dd, yyyy"));
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Birthday cannot be set in the future.") == 1);

            userProfilePage.FillAddressInput("0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 ");
            wait.Until(d => userProfilePage.ValidationErrors.Any());
            Assert.True(userProfilePage.ValidationErrors.Count(e => e.Text == "Max characters for Address name is 100.") == 1);
        }

        [Fact]
        public void ChangePassword()
        {
            userProfilePage.SignIn("the_flash", "password");
            wait.Until(d => d.Url == $"{rootUrl}/#/chat");
            Assert.Equal($"{rootUrl}/#/chat", driver.Url);

            userProfilePage.ClickUserProfileButton();
            wait.Until(d => userProfilePage.ChangePasswordLink.Displayed);

            userProfilePage.ClickChangePasswordLink();
            wait.Until(d => userProfilePage.ConfirmationDialog.Displayed);
            Assert.True(userProfilePage.ConfirmationDialog.Displayed);
            Assert.Equal("Reset Password", userProfilePage.GetConfirmDiagTitle());
            Assert.Equal("We will send a password reset link to your email. Do you want to continue?", userProfilePage.GetConfirmDiagMessage());
        }

        [Fact]
        public void Logout()
        {
            userProfilePage.SignIn("the_flash", "password");
            wait.Until(d => d.Url == $"{rootUrl}/#/chat");
            Assert.Equal($"{rootUrl}/#/chat", driver.Url);

            userProfilePage.ClickUserProfileButton();
            wait.Until(d => userProfilePage.ChangePasswordLink.Displayed);

            userProfilePage.LogoutLink.Click();
            wait.Until(d => d.Url == $"{rootUrl}/#/signin");
            Assert.Equal($"{rootUrl}/#/signin", driver.Url);
        }
    }
}
