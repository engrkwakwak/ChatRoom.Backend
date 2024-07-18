using ChatRoom.UITest.Pages;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

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
            signInPage = new SignInPage(driver);
            signUpPage = new SignUpPage(driver);
        }

        public void Dispose()
        {
            driver.Dispose();
        }

        [Fact]
        public void SignUp_WithValidData_RedirectsToSignIn()
        {
            signUpPage.NavigateToSignUp();
            signUpPage.PopulateForm("Test User", "testuser", "test@email.com", "password", "password");
            signUpPage.SubmitSignUpForm();
            string oldUrl = driver.Url;
            wait.Until(d => d.Url != oldUrl);

            Assert.Matches($"{rootUrl}/#/signin", driver.Url);
        }

        [Fact]
        public void SignIn_WithValidData_RedirectsToEmailVerification()
        {
            signInPage.NavigateToSignIn();
            signInPage.PopulateForm("testuser", "password");
            signInPage.SubmitSignInForm();
            string oldUrl = driver.Url;
            wait.Until(d => d.Url != oldUrl);

            Assert.Equal($"{rootUrl}/#/email-verification", driver.Url);
        }

        [Fact]
        public void SignIn_WithInvalidData_DisplaysValidationErrors()
        {
            signInPage.NavigateToSignIn();
            Assert.Equal($"{rootUrl}/#/signin", driver.Url);

            signInPage.FillUsernameInput("");
            signInPage.FillPasswordInput("");
            signInPage.SubmitSignInForm();
            wait.Until(d => signInPage.ValidationErrors.Count() == 2);
            Assert.True(signInPage.ValidationErrors.Count(e => e.Text == "Username is required.") == 1);
            Assert.True(signInPage.ValidationErrors.Count(e => e.Text == "Password is required.") == 1);

            signInPage.FillUsernameInput("invalidusername");
            signInPage.FillPasswordInput("invalidpassword");
            signInPage.SubmitSignInForm();
            wait.Until(d =>
            {
                try
                {
                    return signInPage.InvalidAuthAlert.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });
            Assert.Equal("Invalid username or password.", signInPage.InvalidAuthAlert.Text);
        }

        [Fact]
        public void SignUp_WithInvalidData_DisplaysValidationError()
        {
            signUpPage.NavigateToSignUp();
            Assert.Equal($"{rootUrl}/#/signup", driver.Url);

            signUpPage.FillDisplayNameInput("");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Display Name is required.") == 1);

            signUpPage.FillDisplayNameInput("1234567890 1234567890 1234567890 1234567890 1234567890 ");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Max characters for Display name is 50.") == 1);

            signUpPage.FillUsernameInput("");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Username is required.") == 1);

            signUpPage.FillUsernameInput("1234567890 1234567890 1234567890");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Max characters for Username is 20.") == 1);

            signUpPage.FillUsernameInput("batman");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Username is already taken") == 1);

            signUpPage.FillUsernameInput("bat man");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Username should not include spaces.") == 1);

            signUpPage.FillEmailInput("");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Email is required.") == 1);

            signUpPage.FillEmailInput("0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 ");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Max characters for Email is 100.") == 1);

            signUpPage.FillEmailInput("INVALIDEMAIL");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Email is not valid.") == 1);

            signUpPage.FillEmailInput("brucewaynes@test.com");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Email is already taken.") == 1);

            signUpPage.FillPasswordInput("");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Password is required.") == 1);

            signUpPage.FillPasswordInput("passwor");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Password must be atleast 8 characters long.") == 1);
            signUpPage.FillPasswordInput("password");

            signUpPage.FillPasswordConfirmationInput("");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Password is required.") == 1);

            signUpPage.FillPasswordConfirmationInput("passwor");
            wait.Until(d => signUpPage.ValidationErrors.Any());
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Password must be atleast 8 characters long.") == 1);
            Assert.True(signUpPage.ValidationErrors.Count(e => e.Text == "Password does not match.") == 1);
        }
    }
}
