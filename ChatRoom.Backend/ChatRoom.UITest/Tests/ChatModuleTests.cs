using ChatRoom.UITest.Pages;
using FluentAssertions;

namespace ChatRoom.UITest.Tests; 
public class ChatModuleTests : TestSetup {

    [Fact]
    public void CreateChat_EmptyChatName_ShouldDisplayValidationMessage() {
        // Arrange
        var chatsPage = new ChatsPage(Driver).Navigate(BaseUrl);

        // Act
        chatsPage.ClickCreateChat()
            .PopulateChatName("")
            .ClickCreateChatContinue();
        
        // Assert
        string validationMessage = chatsPage.GetValidationMessage();
        validationMessage.Should().Be("*Group name is required.");
    }

    [Fact]
    public void CreateChat_ChatNameGreaterThan50Characters_ShouldDisplayValidationMessage() {
        // Arrange
        var chatsPage = new ChatsPage(Driver).Navigate(BaseUrl);

        // Act
        chatsPage.ClickCreateChat()
            .PopulateChatName("1234567890 1234567890 1234567890 1234567890 1234567890")
            .ClickCreateChatContinue();

        // Assert
        string validationMessage = chatsPage.GetValidationMessage();
        validationMessage.Should().Be("*Group Name cannot be more than 50 characters long.");
    }

    [Fact]
    public void CreateChat_WhenCloseButtonIsClicked_ShouldCloseTheModalView() {
        // Arrange
        var chatsPage = new ChatsPage(Driver).Navigate(BaseUrl);

        // Act
        chatsPage.ClickCreateChat()
            .PopulateChatName("1234567890 1234567890")
            .ClickClose();

        // Assert
        chatsPage.IsChatNameFieldPresent().Should().BeFalse();
    }

    [Fact]
    public void AddMembers_NoMemberSelected_ShouldDisableTheCompleteButton() {
        // Arrange
        var chatsPage = new ChatsPage(Driver).Navigate(BaseUrl);

        // Act
        chatsPage.ClickCreateChat()
            .PopulateChatName("1234567890")
            .ClickCreateChatContinue();

        // Assert
        chatsPage.IsAddMemberCompleteButtonEnabled().Should().BeFalse();
    }

    [Fact]
    public void AddMembers_WhenCloseButtonIsClicked_ShouldCloseTheModalView() {
        // Arrange
        var chatsPage = new ChatsPage(Driver).Navigate(BaseUrl);

        // Act
        chatsPage.ClickCreateChat()
            .PopulateChatName("1234567890")
            .ClickCreateChatContinue()
            .ClickClose();

        // Assert
        chatsPage.IsAddMemberCompleteButtonPresent().Should().BeFalse();
    }

    [Fact]
    public void AddMembers_WhenCompleteButtonIsClicked_ShouldRedirectToTheNewlyCreatedChat() {
        // Arrange
        var chatsPage = new ChatsPage(Driver).Navigate(BaseUrl);
        string chatName = Guid.NewGuid().ToString();

        // Act
        chatsPage.ClickCreateChat()
            .PopulateChatName(chatName)
            .ClickCreateChatContinue()
            .SelectFirstFilteredUser() // Check the first user.
            .ClickAddMemberComplete();

        // Assert
        Driver.Url.Should().ContainAll($"{BaseUrl}/#/chat/view/from-chatlist/");
    }

    [Theory]
    [InlineData("steve")]
    [InlineData("allen")]
    public void AddMembers_FilterMembersByName_ShouldOnlyDisplayMatchingNames(string keyword) {
        // Arrange
        var chatsPage = new ChatsPage(Driver).Navigate(BaseUrl);
        string chatName = Guid.NewGuid().ToString();

        // Act
        chatsPage.ClickCreateChat()
            .PopulateChatName(chatName)
            .ClickCreateChatContinue()
            .PopulateSearch(keyword);

        // Assert
        chatsPage.AreFilteredUsernamesMatching(keyword).Should().BeTrue();
    }

    [Fact]
    public void ChatManagement_ClickChatSettingsButton_ShouldOpenTheModalForChatSettings() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage.ClickChatSettings();

        // Assert
        chatManagementPage.GetChatName().Should().Be(name);
    }

    [Fact]
    public void ChatManagement_ChangeChatName_ShouldUpdateTheChatName() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string newName = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage
            .ClickChatSettings()
            .PopulateChatName(newName)
            .ClickUpdateChat(newName)
            .ClickClose(); 

        // Assert
        chatManagementPage.GetChatTitleName().Should().Be(newName);
    }

    [Fact]
    public void ChatManagement_ViewMembers_ShouldOpenAnotherModalForTheMembers() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage
            .ClickChatSettings()
            .ClickMembers();

        // Assert
        chatManagementPage.GetChatMemberTitle().Should().Be("Members");
    }

    [Fact]
    public void ChatManagement_LeaveChat_WhenTheUserIsTheOnlyAdmin_ShouldRestrictTheUserFromLeaving() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage
            .ClickChatSettings()
            .ClickLeaveChat();

        // Assert
        chatManagementPage.GetChatTitleName().Should().Be(name);
    }

    [Fact]
    public void ChatManagement_LeaveChat_WhenThereIsOtherChatAdmin_ShouldSuccessfullyLeaveTheChat() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage
            .ClickChatSettings()
            .ClickMembers()
            .SetFirstMemberAsAdmin()
            .ClickClose()
            .ClickChatSettings()
            .ClickLeaveChat();

        // Assert
        chatManagementPage.IsChatTitlePresent().Should().BeFalse();
    }

    [Fact]
    public void ChatManagement_DeleteChat_ShouldRemoveTheChatInTheChatList() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage
            .ClickChatSettings()
            .ClickDeleteChat(name);

        // Assert
        chatManagementPage.IsChatTitlePresent().Should().BeFalse();
    }

    [Fact]
    public void ChatMembersModal_SetAsAdmin_ShouldChangeRoleToAdmin() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage
            .ClickChatSettings()
            .ClickMembers()
            .SetFirstMemberAsAdmin();

        // Assert
        chatManagementPage.GetFirstMemberRole().Should().Be("Admin");
    }

    [Fact]
    public void ChatMembersModal_RemoveMember_ShouldRemoveMemberToTheChat() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        string removedUser = chatManagementPage
            .ClickChatSettings()
            .ClickMembers()
            .RemoveFirstMemberOfTheGroup();

        // Assert
        string message = $"{SessionUserName} removed {removedUser} from the group.";
        chatManagementPage.CheckIfMessageExists(message).Should().BeTrue();
    }

    [Fact]
    public void ChatMembersModal_SearchUser_ShouldDisplayUsersWithMatchingNames() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage
            .ClickChatSettings()
            .ClickMembers()
            .SearchUser("steve");

        // Assert
        chatManagementPage.CheckIfAllMembersHasMatchingName("steve");
    }

    [Fact]
    public void ChatMembersModal_ClickCloseButton_ShouldCloseTheModal() {
        // Arrange
        string name = Guid.NewGuid().ToString();
        string url = CreateChat(name);
        var chatManagementPage = new ChatManagementPage(Driver).Navigate(url);

        // Act
        chatManagementPage
            .ClickChatSettings()
            .ClickMembers()
            .ClickClose();

        // Assert
        chatManagementPage.IsChatNameFieldPresent().Should().BeFalse();
    }
}
