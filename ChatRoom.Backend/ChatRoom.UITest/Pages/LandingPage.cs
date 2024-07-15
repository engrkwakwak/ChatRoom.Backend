using OpenQA.Selenium;

namespace ChatRoom.UITest.Pages; 
public class LandingPage {
    private readonly IWebDriver _driver;
    private const string PageUrl = "/chat";

    public LandingPage(IWebDriver driver) {
        _driver = driver;
    }

    //public IWebElement CreateChatButton => _driver.FindElement(By)
}
