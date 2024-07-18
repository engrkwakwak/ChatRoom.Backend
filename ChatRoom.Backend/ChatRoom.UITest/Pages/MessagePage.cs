using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace ChatRoom.UITest.Pages;
public class MessagePage(IWebDriver driver) {
    private readonly IWebDriver _driver = driver;
    private readonly TimeSpan _searchTimeout = TimeSpan.FromSeconds(5);

    private IWebElement GetMessageInputField() {
        var wait = new WebDriverWait(_driver, _searchTimeout);
        return wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[placeholder='Type a message']")));
    }

    private IWebElement GetSendButton() {
        var wait = new WebDriverWait(_driver, _searchTimeout);
        return wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("button.send-button")));
    }

    private IWebElement GetMessageListContainer() {
        var wait = new WebDriverWait(_driver, _searchTimeout);
        return wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div .messages")));
    }

    private IWebElement GetUpdateMessageButton(IWebElement parentElement) {
        var wait = new WebDriverWait(_driver, _searchTimeout);
        return wait.Until(driver => {
            return parentElement.FindElement(By.XPath(".//button[normalize-space(text())='Edit']"));
        });
    }

    private IWebElement GetDeleteMessageButton(IWebElement parentElement) {
        var wait = new WebDriverWait(_driver, _searchTimeout);
        return wait.Until(driver => {
            return parentElement.FindElement(By.XPath(".//button[normalize-space(text())='Delete']"));
        });
    }

    private IWebElement UpdateMessageInputField {
        get {
            var wait = new WebDriverWait(_driver, _searchTimeout);
            return wait.Until(ExpectedConditions.ElementIsVisible(By.Id("update-message-input")));
        }
    }

    private IWebElement SaveChangesButton {
        get {
            var wait = new WebDriverWait(_driver, _searchTimeout);
            return wait.Until(ExpectedConditions.ElementIsVisible(By.Id("save-changes-button")));
        }
    }

    private IWebElement CloseUpdateButton {
        get {
            var wait = new WebDriverWait(_driver, _searchTimeout);
            return wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(".//button[@nbbutton and @ghost and .//nb-icon[@icon='close-outline']]")));
        }
    }

    private IEnumerable<IWebElement> NormalMessages {
        get {
            var wait = new WebDriverWait(_driver, _searchTimeout);
            return wait.Until(driver => {
                return _driver.FindElements(By.CssSelector("div .message-text"));
            });
        }
    }

    public MessagePage Navigate(string url) {
        _driver.Navigate().GoToUrl(url);
        return this;
    }

    public MessagePage SendAMessage(string message) {
        var messageInput = GetMessageInputField();
        var sendButton = GetSendButton();
        GetMessageListContainer();

        messageInput.Clear();
        messageInput.SendKeys(message);
        sendButton.Click();

        return this;
    }

    public MessagePage UpdateAMessage(string message, string updatedMessage) {
        var messageList = GetMessageListContainer();
        var messages = messageList.FindElements(By.CssSelector("div.content.reply"));
        var messageElement = GetMessageElementByMessageContent(messages, message);

        var updateMessageButton = GetUpdateMessageButton(messageElement);
        updateMessageButton.Click();

        UpdateMessageInputField.Clear();
        UpdateMessageInputField.SendKeys(updatedMessage);
        SaveChangesButton.Click();

        return this;
    }

    public MessagePage DeleteAMessage(string message) {
        var messageList = GetMessageListContainer();
        var messages = messageList.FindElements(By.CssSelector("div.content.reply"));
        var messageElement = GetMessageElementByMessageContent(messages, message);

        var deleteMessageButton = GetDeleteMessageButton(messageElement);
        deleteMessageButton.Click();

        var continueButton = _driver.FindElement(By.Id("continue-button"));
        continueButton.Click();

        var wait = new WebDriverWait(_driver, _searchTimeout);
        wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id("continue-button")));

        return this;
    }

    public MessagePage CloseUpdateMessageMenu() {
        CloseUpdateButton.Click();

        return this;
    }

    public bool IsMessageSent(string message) {
        return IsMessagePresentInMessageList(message);
    }

    public bool IsMessageUpdated(string message, string updatedMessage) {
        return !IsMessagePresentInMessageList(message) && IsMessagePresentInMessageList(updatedMessage);
    }

    public bool IsMessageDeleted(string message) {
        return !IsMessagePresentInMessageList(message);
    }

    private bool IsMessagePresentInMessageList(string contentToSearch) {
        foreach(var message in NormalMessages) {
            if(message.Text.Contains(contentToSearch)) 
                return true;
        }
        return false;
    }

    private static IWebElement GetMessageElementByMessageContent(IEnumerable<IWebElement> messages, string content) {
        foreach (var message in messages) {
            var messageContent = message.FindElement(By.CssSelector("div.message-text"));
            var messageContentText = messageContent.Text;

            if (messageContentText.Contains(content)) {
                return message;
            }
        }

        throw new NoSuchElementException();
    }
}
