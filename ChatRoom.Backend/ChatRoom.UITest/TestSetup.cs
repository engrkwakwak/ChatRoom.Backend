using ChatRoom.UITest.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ChatRoom.UITest; 
public abstract class TestSetup : IDisposable {
    protected const string BaseUrl = Environments.FRONTEND_URI;
    protected const string SessionUserName = "Tony Stark";
    protected IWebDriver Driver { get; private set; }
    public TestSetup() {
        var chromeOptions = new ChromeOptions();
        //chromeOptions.AddArgument("--headless"); // Run in headless mode
        chromeOptions.AddArgument("--window-size=1920,1080");

        Driver = new ChromeDriver(chromeOptions);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        LoginToApplication("iron_man", "password");
    }

    protected void LoginToApplication(string username, string password) {
        var signinPage = new SignInPage(Driver);
        signinPage.Navigate();
        signinPage.PopulateForm(username, password);
        signinPage.SubmitSignInForm();
    }

    public void Dispose() {
        Driver.Quit();
    }

    protected string CreateChat(string chatName) {
        var chatsPage = new ChatsPage(Driver).Navigate(BaseUrl);
        chatsPage.ClickCreateChat()
            .PopulateChatName(chatName)
            .ClickCreateChatContinue()
            .SelectFirstFilteredUser() // Check the first user.
            .ClickAddMemberComplete();

        Driver.Url.Should().ContainAll($"{BaseUrl}/#/chat/view/from-chatlist/");

        return Driver.Url;
    }
}
