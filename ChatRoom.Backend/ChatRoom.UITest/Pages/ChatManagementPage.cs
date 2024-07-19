using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Xml.Linq;

namespace ChatRoom.UITest.Pages; 
public class ChatManagementPage {
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public ChatManagementPage(IWebDriver driver) {
        _driver = driver;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
    }

    public string PageSource => _driver.PageSource;

    public IWebElement MembersButton => _driver.FindElement(By.Id("members-button"));
    public IWebElement LeaveChatButton => _driver.FindElement(By.Id("leave-chat-button"));
    public IWebElement DeleteChatButton => _driver.FindElement(By.Id("delete-chat-button"));
    public IWebElement ContinueConfirmationButton => _driver.FindElement(By.Id("continue_btn"));
    public IWebElement CloseButton => _driver.FindElement(By.Id("close-button"));
    public IWebElement ChatSettingsButton => _driver.FindElement(By.Id("chat-settings-button"));
    public IWebElement UpdateChatButton => _driver.FindElement(By.Id("update-chat-button"));
    public IWebElement ChatTitleField => _driver.FindElement(By.CssSelector("span.fw-bold"));
    public IWebElement ChatNameField => _driver.FindElement(By.Id("chat-name"));
    public IWebElement ChatMembersTitleField => _driver.FindElement(By.CssSelector("div.section-title"));
    public IWebElement ChatMemberRoleField => _driver.FindElement(By.CssSelector("p.chat-member-role"));
    public IReadOnlyCollection<IWebElement> GetChatMemberItems() {
        var chatMembersList = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".chat-member-list")));
        return chatMembersList.FindElements(By.TagName("app-chat-member-item"));
    }

    public ChatManagementPage Navigate(string url) {
        _driver.Navigate().GoToUrl(url);
        return this;
    }

    public ChatManagementPage ClickChatSettings() {
        ChatSettingsButton.Click();
        return this;
    }

    public ChatManagementPage ClickClose() {
        CloseButton.Click();

        return this;
    }

    public ChatManagementPage ClickMembers() {
        _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("members-button")));
        MembersButton.Click();
        return this;
    }

    public ChatManagementPage ClickLeaveChat() {
        LeaveChatButton.Click();
        ContinueConfirmationButton.Click();

        _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id("continue_btn")));
        
        return this;
    }

    public ChatManagementPage ClickContinueConfirmation() {
        ContinueConfirmationButton.Click();
        return this;
    }

    public ChatManagementPage ClickDeleteChat(string chatName) {
        DeleteChatButton.Click();
        ContinueConfirmationButton.Click();

        _wait.Until(ExpectedConditions.InvisibilityOfElementWithText(By.CssSelector("span.fw-bold"), chatName));

        return this;
    }

    public string GetChatTitleName() {
        return ChatTitleField.Text;
    }

    public string GetChatMemberTitle() {
        return ChatMembersTitleField.Text;
    }

    public string GetChatName() {
        return ChatNameField.GetAttribute("value");
    }
    public string GetFirstMemberRole() {
        var chatMembersList = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".chat-member-list")));
        var firstMemberItem = chatMembersList.FindElements(By.TagName("app-chat-member-item"))[0];
        var memberRoleElement = firstMemberItem.FindElement(By.CssSelector(".chat-member-role"));
        return memberRoleElement.Text;
    }

    public ChatManagementPage PopulateChatName(string name) {
        ChatNameField.Clear();
        ChatNameField.SendKeys(name);
        return this;
    }

    public ChatManagementPage ClickUpdateChat(string newName) {
        IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
        js.ExecuteScript("arguments[0].click();", UpdateChatButton);

        _wait.Until(driver => {
            return ChatTitleField.Text == newName;
        });

        return this;
    }

    public bool IsChatTitlePresent() {
        try {
            _driver.FindElement(By.CssSelector("span.fw-bold"));
            return true;
        }
        catch (NoSuchElementException) {
            return false;
        }
    }

    public bool IsChatNameFieldPresent() {
        try {
            _driver.FindElement(By.Id("chat-name"));
            return true;
        }
        catch (NoSuchElementException) {
            return false;
        }
    }

    public ChatManagementPage SetFirstMemberAsAdmin() {
        var chatMembersList = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".chat-member-list")));

        var firstMemberItem = chatMembersList.FindElements(By.TagName("app-chat-member-item"))[0];

        var userActionsButton = firstMemberItem.FindElement(By.CssSelector("button.member-menu-button"));
        userActionsButton.Click();

        var setAsAdminOption = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//nb-list-item[contains(text(), 'Set as Admin')]")));
        setAsAdminOption.Click();

        _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//nb-list-item[contains(text(), 'Remove as Admin')]")));

        return this;
    }

    public string RemoveFirstMemberOfTheGroup() {
        var chatMembersList = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".chat-member-list")));

        var firstMemberItem = chatMembersList.FindElements(By.TagName("app-chat-member-item"))[0];
        var memberNameElement = firstMemberItem.FindElement(By.CssSelector(".chat-member-name"));
        string memberName = memberNameElement.Text;

        var userActionsButton = firstMemberItem.FindElement(By.CssSelector("button.member-menu-button"));
        userActionsButton.Click();

        var removeMemberOption = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//nb-list-item[contains(text(), 'Remove Member')]")));
        removeMemberOption.Click();

        _wait.Until(ExpectedConditions.InvisibilityOfElementWithText(By.CssSelector(".chat-member-name"), memberName));

        return memberName;
    }

    public bool CheckIfMessageExists(string message) {
        var messagesDiv = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.messages")));
        string messagesText = messagesDiv.Text;

        bool messageExists = messagesText.Contains(message);
        return messageExists;
    }

    public ChatManagementPage SearchUser(string name) {
        var searchInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("search-input")));
        searchInput.Clear();
        searchInput.SendKeys(name);

        _wait.Until(driver => {
            var chatMembersList = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".available-user-list")));
            var firstMemberItem = chatMembersList.FindElements(By.TagName("app-chat-member-item"))[0];
            var memberNameElement = firstMemberItem.FindElement(By.CssSelector(".user-name"));
            string memberName = memberNameElement.Text.ToLower();

            return memberName.Contains(name);
        });

        return this;
    }

    public bool CheckIfAllMembersHasMatchingName(string name) {
        var chatMembersList = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".available-user-list")));
        var memberItems = chatMembersList.FindElements(By.TagName("app-chat-member-item"));

        foreach (var member in memberItems) {
            var memberNameElement = member.FindElement(By.CssSelector(".user-name"));
            string memberName = memberNameElement.Text;

            if (!memberName.Contains(name)) {
                return false;
            }
        }
        return true;
    }
}
