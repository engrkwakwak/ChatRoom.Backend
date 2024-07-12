using ChatRoom.Backend.Presentation.Controllers;
using ChatRoom.Backend.Presentation.Hubs;
using Entities.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Messages;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;
using System.Text.Json;

namespace ChatRoom.UnitTest.ControllerTests;
public class MessagesControllerTests {
    private readonly Mock<IServiceManager> _mockService;
    private readonly Mock<IHubContext<ChatRoomHub>> _mockHub;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly MessagesController _controller;

    public MessagesControllerTests() {
        _mockService = new Mock<IServiceManager>();
        _mockHub = new Mock<IHubContext<ChatRoomHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _controller = new MessagesController(_mockService.Object, _mockHub.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetMessagesForChat_ServiceReturnsEmptyMessageList_ShouldReturnOkStatus() {
        // Arrange
        int chatId = 1;
        MessageParameters messageParameters = new();

        IEnumerable<MessageDto> messages = [];
        MetaData metaData = new();

        _mockService.Setup(x => x.MessageService.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync((messages, metaData));

        // Act
        var result = await _controller.GetMessagesForChat(chatId, messageParameters) as ObjectResult;

        // Assert
        result.Should().NotBeNull();    
        result.Should().BeAssignableTo<OkObjectResult>();
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetMessagesForChat_ServiceReturnsEmptyMessageList_ShouldReturnEmptyMessageList() {
        // Arrange
        int chatId = 1;
        MessageParameters messageParameters = new();

        IEnumerable<MessageDto> messages = [];
        MetaData metaData = new();

        _mockService.Setup(x => x.MessageService.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync((messages, metaData));

        // Act
        var result = await _controller.GetMessagesForChat(chatId, messageParameters) as ObjectResult;

        // Assert
        var resultValue = result?.Value as IEnumerable<MessageDto>;
        resultValue.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMessagesForChat_ServiceReturnsEmptyMessageList_ShouldAppendMetaDataToHeaders() {
        // Arrange
        int chatId = 1;
        MessageParameters messageParameters = new();

        IEnumerable<MessageDto> messages = [];
        MetaData metaData = new();

        _mockService.Setup(x => x.MessageService.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync((messages, metaData));

        // Act
        var result = await _controller.GetMessagesForChat(chatId, messageParameters) as ObjectResult;

        // Assert
        _controller.Response.Headers.Should().ContainKey("X-Pagination");
        _controller.Response.Headers["X-Pagination"].ToString().Should().Be(JsonSerializer.Serialize(metaData));
    }

    [Fact]
    public async Task GetMessagesForChat_ServiceReturnsMessageList_ShouldReturnOkStatus() {
        // Arrange
        int chatId = 1;
        MessageParameters messageParameters = new();

        IEnumerable<MessageDto> messages = CreateMessages();
        MetaData metaData = new();

        _mockService.Setup(x => x.MessageService.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync((messages, metaData));

        // Act
        var result = await _controller.GetMessagesForChat(chatId, messageParameters) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<OkObjectResult>();
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetMessagesForChat_ServiceReturnsMessageList_ShouldReturnMessageList() {
        // Arrange
        int chatId = 1;
        MessageParameters messageParameters = new();

        IEnumerable<MessageDto> messages = CreateMessages();
        MetaData metaData = new();

        _mockService.Setup(x => x.MessageService.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync((messages, metaData));

        // Act
        var result = await _controller.GetMessagesForChat(chatId, messageParameters) as ObjectResult;

        // Assert
        var resultValue = result?.Value as IEnumerable<MessageDto>;
        resultValue.Should().BeEquivalentTo(messages);
    }

    [Fact]
    public async Task GetMessagesForChat_ServiceReturnsMessageList_ShouldAppendMetaDataToHeaders() {
        // Arrange
        int chatId = 1;
        MessageParameters messageParameters = new();

        IEnumerable<MessageDto> messages = CreateMessages();
        MetaData metaData = new();

        _mockService.Setup(x => x.MessageService.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync((messages, metaData));

        // Act
        var result = await _controller.GetMessagesForChat(chatId, messageParameters) as ObjectResult;

        // Assert
        _controller.Response.Headers.Should().ContainKey("X-Pagination");
        _controller.Response.Headers["X-Pagination"].ToString().Should().Be(JsonSerializer.Serialize(metaData));
    }

    [Fact]
    public async Task SendMessage_ServiceReturnsEmptyChatMembers_ShouldThrowNotFoundException() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        ChatDto chat = CreateP2PChatDto();

        IEnumerable<ChatMemberDto> members = [];

        _mockService.Setup(x => x.ChatService.GetChatByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(chat);
        _mockService.Setup(x => x.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(members);

        // Act
        Func<Task> act = async() => await _controller.SendMessage(messageToSend);

        // Assert
        await act.Should().ThrowAsync<NoChatMembersFoundException>()
            .WithMessage($"No members found for chat with chat id {messageToSend.ChatId}.");
    }

    [Fact]
    public async Task SendMessage_ChatTypeIsP2P_ShouldInsertReceiverToContacts() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        ChatDto chat = CreateP2PChatDto();

        IEnumerable<ChatMemberDto> members = CreateChatMembers();

        _mockService.Setup(x => x.ChatService.GetChatByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(chat);
        _mockService.Setup(x => x.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(members);
        _mockService.Setup(x => x.ContactService.InsertContactsAsync(It.IsAny<int>(), It.IsAny<List<int>>()))
            .ReturnsAsync([]);

        // Act
        await _controller.SendMessage(messageToSend);

        // Assert
        _mockService.Verify(x => x.ContactService.InsertContactsAsync(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Once);
    }

    [Fact]
    public async Task SendMessage_ChatTypeIsP2P_ShouldNotifyTheSenderTheContactIsAdded() {

    }

    [Fact]
    public async Task SendMessage_ChatTypeIsGroupChat_ShouldNotInsertContacts() {

    }

    [Fact]
    public async Task SendMessage_ServiceSuccessfullyInsertedTheMessage_ShouldNotifyMembersToReceiveNewMessage() {

    }

    [Fact]
    public async Task SendMessage_ServiceSuccessfullyInsertedTheMessage_ShouldReturnMessageDto() {

    }

    [Fact]
    public async Task SendMessage_ServiceSuccessfullyInsertedTheMessage_ShouldReturnTheMessageInfoCorrectly() {

    }

    [Fact]
    public async Task GetLatestMessage_ServiceReturnsEmptyMessageList_ShouldReturnNull() {

    }

    [Fact]
    public async Task GetLatestMessage_ServiceReturnsMessageSuccessfully_ShouldReturnOk() {

    }

    [Fact]
    public async Task GetLatestMessage_ServiceReturnsMessageSuccessfully_ShouldReturnMessageDto() {

    }

    [Fact]
    public async Task Delete_MessageSenderIsNotTheLoggedInUser_ShouldThrowUnauthorizedException() {

    }

    [Fact]
    public async Task Delete_MessageSenderIsTheLoggedInUser_ShouldDeleteTheMessage() {

    }

    [Fact]
    public async Task Delete_ServiceSuccessfullyDeletesTheMessage_ShouldNotifyGroupTheMessageIsDeleted() {

    }

    [Fact]
    public async Task Delete_ServiceSuccessfullyDeletesTheMessage_ShouldReturnNoContent() {

    }

    [Fact]
    public async Task Update_MessageSenderIsNotTheLoggedInUser_ShouldThrowUnauthorizedException() {

    }

    [Fact]
    public async Task Update_MessageSenderIsTheLoggedInUser_ShouldUpdateTheMessage() {

    }

    [Fact]
    public async Task Update_ServiceSuccessfullyUpdatesTheMessage_ShouldNotifyGroupTheMessageIsUpdated() {

    }

    [Fact]
    public async Task Update_ServiceSuccessfullyUpdatesTheMessage_ShouldReturnNoContent() {

    }

    private static IEnumerable<MessageDto> CreateMessages() {
        return [
            new MessageDto(),
            new MessageDto(),
            new MessageDto()
        ];
    }

    private static IEnumerable<ChatMemberDto> CreateChatMembers() {
        return [
            new ChatMemberDto() {
                ChatId = 1,
                IsAdmin = true,
                LastSeenMessageId = 1,
                StatusId = 1,
                User = new UserDisplayDto {
                    UserId = 1,
                }
            },
            new ChatMemberDto {
                ChatId = 1,
                IsAdmin = true,
                LastSeenMessageId = 1,
                StatusId = 1,
                User = new UserDisplayDto {
                    UserId = 2,
                }
            }
        ];
    }

    private static MessageForCreationDto CreateMessageForCreationDto() {
        return new MessageForCreationDto {
            ChatId = 1,
            Content = "Content",
            MsgTypeId = 1,
            SenderId = 1
        };
    }

    private static ChatDto CreateP2PChatDto() {
        return new ChatDto {
            ChatId = 1,
            ChatTypeId = 1,
            ChatName = "Chat Name"
        };
    }
}
