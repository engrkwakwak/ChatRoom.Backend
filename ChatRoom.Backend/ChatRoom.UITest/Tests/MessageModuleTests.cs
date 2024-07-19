using ChatRoom.UITest.Pages;
using FluentAssertions;

namespace ChatRoom.UITest.Tests; 
public class MessageModuleTests : TestSetup {
    /*  Test setup coverage:
     *  1. Logging in to the application
     *  2. GroupChat creation.
     */

    private string ChatUrl { get; set; }
    public MessageModuleTests() {
        string chatName = Guid.NewGuid().ToString();
        ChatUrl = CreateChat(chatName);
    }

    [Fact]
    public void SendMessage_MessageHasValidContent_ShouldDisplayTheMessageInMessageList() {
        // Arrange
        var messagePage = new MessagePage(Driver).Navigate(ChatUrl);
        string messageToSend = "Sample valid message.";

        // Act
        messagePage.SendAMessage(messageToSend);

        // Assert
        messagePage.IsMessageSent(messageToSend).Should().BeTrue();
    }

    [Fact]
    public void SendMessage_MessageIsMoreThan500Characters_ShouldNotSendTheMessage() {
        // Arrange
        var messagePage = new MessagePage(Driver).Navigate(ChatUrl);
        string messageToSend = "Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. " +
            "Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. " +
            "Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. " +
            "Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. ";

        // Act
        messagePage.SendAMessage(messageToSend);

        // Assert
        messagePage.IsMessageSent(messageToSend).Should().BeFalse();
    }

    [Fact]
    public void UpdateMessage_MessageHasValidContent_ShouldDisplayTheUpdatedMessageInTheMessageList() {
        // Arrange
        var messagePage = new MessagePage(Driver).Navigate(ChatUrl);
        string messageToSend = "Sample valid message.";
        string updatedMessage = "Updated sample valid message";

        // Act
        messagePage.SendAMessage(messageToSend)
            .UpdateAMessage(messageToSend, updatedMessage);

        // Assert
        messagePage.IsMessageUpdated(messageToSend, updatedMessage).Should().BeTrue();
    }

    [Fact]
    public void UpdateMessage_MessageIsMoreThan500Characters_ShouldNotUpdateTheMessage() {
        // Arrange
        var messagePage = new MessagePage(Driver).Navigate(ChatUrl);
        string messageToSend = "Sample valid message.";
        string updatedMessage = "Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. " +
            "Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. " +
            "Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. " +
            "Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. Message is more than 500 characters. ";

        // Act
        messagePage.SendAMessage(messageToSend)
            .UpdateAMessage(messageToSend, updatedMessage)
            .CloseUpdateMessageMenu();

        // Assert
        messagePage.IsMessageUpdated(messageToSend, updatedMessage).Should().BeFalse();
    }

    [Fact]
    public void DeleteMessage_ShouldRemoveTheMessageInTheMessageList() {
        // Arrange
        var messagePage = new MessagePage(Driver).Navigate(ChatUrl);
        string messageToSend = "Sample valid message.";

        // Act
        messagePage.SendAMessage(messageToSend)
            .DeleteAMessage(messageToSend);

        // Assert
        messagePage.IsMessageDeleted(messageToSend);
    }
}
