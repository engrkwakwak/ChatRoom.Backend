using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ChatRoom.UITest.Pages; 
public class ChatsPage {
    private readonly IWebDriver _driver;
    private const string PageUrl = "/#/chat";
    private readonly TimeSpan _searchTimeout = TimeSpan.FromSeconds(5);

    public ChatsPage(IWebDriver driver) {
        _driver = driver;
    }

    public IWebElement ChatNameField => _driver.FindElement(By.Id("group-name"));
    public IWebElement SearchField => _driver.FindElement(By.Id("search"));
    public IWebElement CreateChatButton => _driver.FindElement(By.Id("create-group-chat-button"));
    public IWebElement CreateChatContinueButton => _driver.FindElement(By.Id("create-group-chat-continue-button"));
    public IWebElement AddMemberCompleteButton => _driver.FindElement(By.Id("add-member-complete-button"));
    public IWebElement CloseButton => _driver.FindElement(By.Id("close-button"));

    private IWebElement ValidationMessageSection => _driver.FindElement(By.CssSelector("div.validation-messages"));
    public IWebElement ValidationMessage => ValidationMessageSection.FindElement(By.CssSelector(".text-danger"));

    public IWebElement FirstFilteredUserCheckbox => _driver.FindElement(By.CssSelector(".filtered-users nb-checkbox:first-child"));
    public IEnumerable<IWebElement> FilteredUserElements => _driver.FindElements(By.CssSelector(".filtered-users app-user-item"));

    public ChatsPage Navigate(string baseUrl) {
        _driver.Navigate().GoToUrl($"{baseUrl}{PageUrl}");
        return this;
    }

    public ChatsPage ClickCreateChat() {
        CreateChatButton.Click();
        return this;
    }

    public ChatsPage ClickCreateChatContinue() {
        CreateChatContinueButton.Click();
        return this;
    }

    public ChatsPage ClickClose() {
        CloseButton.Click();
        return this;
    }

    public ChatsPage ClickAddMemberComplete() {
        AddMemberCompleteButton.Click();

        var wait = new WebDriverWait(_driver, _searchTimeout);
        wait.Until(driver => driver.Url.Contains("from-chatlist"));

        return this;
    }

    public ChatsPage PopulateChatName(string name) {
        ChatNameField.Clear();
        ChatNameField.SendKeys(name);
        return this;
    }

    public ChatsPage PopulateSearch(string keyword) {
        int currentUsers = FilteredUserElements.Count();
        SearchField.Clear();
        SearchField.SendKeys(keyword);

        var wait = new WebDriverWait(_driver, _searchTimeout);
        wait.Until(driver => currentUsers != FilteredUserElements.Count());

        return this;
    }

    public string GetValidationMessage() {
        return ValidationMessage.Text;
    }

    public bool IsAddMemberCompleteButtonEnabled() {
        return !AddMemberCompleteButton.GetAttribute("disabled").Equals("true");
    }

    public bool IsChatNameFieldPresent() {
        try {
            _driver.FindElement(By.Id("group-name"));
            return true;
        }
        catch(NoSuchElementException) {
            return false;
        }
    }

    public bool IsAddMemberCompleteButtonPresent() {
        try {
            _driver.FindElement(By.Id("add-member-complete-button"));
            return true;
        }
        catch (NoSuchElementException) {
            return false;
        }
    }

    public ChatsPage SelectFirstFilteredUser() {
        var firstUserCheckbox = FirstFilteredUserCheckbox;
        if (!firstUserCheckbox.Selected) {
            firstUserCheckbox.Click();
        }
        return this;
    }

    public bool AreFilteredUsernamesMatching(string keyword) {
        return FilteredUserElements.All(userElement =>
        {
            var username = userElement.Text;
            return username.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        });
    }
}
