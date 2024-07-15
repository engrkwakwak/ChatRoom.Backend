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

    private readonly List<(string, object[])> _sendAsyncCalls;

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

        _controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer MockedToken";

        _mockHub.Setup(hub => hub.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(clients => clients.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(clients => clients.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _sendAsyncCalls = new List<(string, object[])>();
        _mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((method, args, token) => _sendAsyncCalls.Add((method, args)))
            .Returns(Task.CompletedTask);
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
        _mockService.Setup(x => x.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>()))
            .ReturnsAsync(new MessageDto());

        // Act
        await _controller.SendMessage(messageToSend);

        // Assert
        _mockService.Verify(x => x.ContactService.InsertContactsAsync(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Once);
    }

    [Fact]
    public async Task SendMessage_ChatTypeIsP2P_ShouldNotifyTheSenderTheContactIsAdded() {
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
        _mockService.Setup(x => x.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>()))
            .ReturnsAsync(new MessageDto());

        // Act
        await _controller.SendMessage(messageToSend);

        // Assert
        _sendAsyncCalls.Should().Contain(call =>
            call.Item1 == "ContactsUpdated" &&
            call.Item2.Length == 0);
    }

    [Fact]
    public async Task SendMessage_ChatTypeIsGroupChat_ShouldNotInsertContacts() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        ChatDto chat = CreateGCChatDto();

        IEnumerable<ChatMemberDto> members = CreateChatMembers();

        _mockService.Setup(x => x.ChatService.GetChatByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(chat);
        _mockService.Setup(x => x.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(members);
        _mockService.Setup(x => x.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>()))
            .ReturnsAsync(new MessageDto());

        // Act
        await _controller.SendMessage(messageToSend);

        // Assert
        _mockService.Verify(x => x.ContactService.InsertContactsAsync(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task SendMessage_ServiceSuccessfullyInsertedTheMessage_ShouldNotifyMembersToReceiveNewMessage() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        ChatDto chat = CreateGCChatDto();

        IEnumerable<ChatMemberDto> members = CreateChatMembers();

        _mockService.Setup(x => x.ChatService.GetChatByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(chat);
        _mockService.Setup(x => x.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(members);
        _mockService.Setup(x => x.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>()))
            .ReturnsAsync(new MessageDto());

        // Act
        await _controller.SendMessage(messageToSend);

        // Assert
        _sendAsyncCalls.Should().Contain(call =>
            call.Item1 == "ReceiveMessage" &&
            call.Item2.Length == 1);
    }

    [Fact]
    public async Task SendMessage_ServiceSuccessfullyInsertedTheMessage_ShouldReturnMessageDto() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        ChatDto chat = CreateGCChatDto();

        IEnumerable<ChatMemberDto> members = CreateChatMembers();

        _mockService.Setup(x => x.ChatService.GetChatByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(chat);
        _mockService.Setup(x => x.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>()))
            .ReturnsAsync(members);
        _mockService.Setup(x => x.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>()))
            .ReturnsAsync(new MessageDto());

        // Act
        var result = await _controller.SendMessage(messageToSend) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeAssignableTo<MessageDto>();
    }

    [Fact]
    public async Task GetLatestMessage_ServiceReturnsEmptyList_ShouldReturnNull() {
        // Arrange
        int chatId = 1;
        MessageParameters msgParams = new();
        IEnumerable<MessageDto> messages = [];
        MetaData metaData = new();
        _mockService.Setup(x => x.MessageService.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync((messages, metaData));

        // Act
        var result = await _controller.GetLatestMessage(chatId) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestMessage_ServiceReturnsMessageList_ShouldReturnOk() {
        // Arrange
        int chatId = 1;
        MessageParameters msgParams = new();
        IEnumerable<MessageDto> messages = CreateMessages();
        MetaData metaData = new();
        _mockService.Setup(x => x.MessageService.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync((messages, metaData));

        // Act & Assert
        var result = await _controller.GetLatestMessage(chatId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(messages.First(), result.Value);
    }

    [Fact]
    public async Task Delete_MessageSenderIsNotTheLoggedInUser_ShouldThrowUnauthorizedException() {
        // Arrange
        int messageId = 1;
        UserDto sessionUser = new() { UserId = 123, DisplayName = "", Email = "", Username = "" };
        UserDisplayDto sender = new() { UserId = 456 };
        MessageDto message = new() { Sender = sender };

        _mockService.Setup(x => x.AuthService.GetUserIdFromJwtToken(It.IsAny<string>()))
            .Returns(123);
        _mockService.Setup(x => x.MessageService.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(message);

        // Act
        Func<Task> act = async() => await _controller.Delete(messageId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedMessageDeletionException>()
            .WithMessage("Deleting messages sent by other users are strictly prohibited.");
    }

    [Fact]
    public async Task Delete_ServiceSuccessfullyDeletesTheMessage_ShouldNotifyGroupTheMessageIsDeleted() {
        // Arrange
        int messageId = 1;
        UserDto sessionUser = new() { UserId = 123, DisplayName = "", Email = "", Username = "" };
        UserDisplayDto sender = new() { UserId = 123 };
        MessageDto message = new() { Sender = sender };

        _mockService.Setup(x => x.AuthService.GetUserIdFromJwtToken(It.IsAny<string>()))
            .Returns(123);
        _mockService.Setup(x => x.MessageService.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(message);
        _mockService.Setup(x => x.MessageService.DeleteMessageAsync(It.IsAny<int>()))
            .ReturnsAsync(message);

        // Act
        var result = await _controller.Delete(messageId);

        // Assert
        _sendAsyncCalls.Should().Contain(call =>
            call.Item1 == "DeleteMessage" &&
            call.Item2.Length == 1);
    }

    [Fact]
    public async Task Delete_ServiceSuccessfullyDeletesTheMessage_ShouldReturnNoContent() {
        // Arrange
        int messageId = 1;
        UserDto sessionUser = new() { UserId = 123, DisplayName = "", Email = "", Username = "" };
        UserDisplayDto sender = new() { UserId = 123 };
        MessageDto message = new() { Sender = sender };

        _mockService.Setup(x => x.AuthService.GetUserIdFromJwtToken(It.IsAny<string>()))
            .Returns(123);
        _mockService.Setup(x => x.MessageService.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(message);
        _mockService.Setup(x => x.MessageService.DeleteMessageAsync(It.IsAny<int>()))
            .ReturnsAsync(message);

        // Act
        var result = await _controller.Delete(messageId);

        // Assert
        result.Should().BeAssignableTo<NoContentResult>();
    }

    [Fact]
    public async Task Update_MessageSenderIsNotTheLoggedInUser_ShouldThrowUnauthorizedException() {
        // Arrange
        int messageId = 1;
        UserDto sessionUser = new() { UserId = 123, DisplayName = "", Email = "", Username = "" };
        UserDisplayDto sender = new() { UserId = 456 };
        MessageForUpdateDto messageForUpdate = CreateMessageForUpdateDto();
        MessageDto message = new() { Sender = sender };

        _mockService.Setup(x => x.AuthService.GetUserIdFromJwtToken(It.IsAny<string>()))
            .Returns(123);
        _mockService.Setup(x => x.MessageService.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(message);

        // Act
        Func<Task> act = async () => await _controller.Update(messageId, messageForUpdate);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedMessageDeletionException>()
            .WithMessage("Updating messages sent by other users are strictly prohibited.");
    }

    [Fact]
    public async Task Update_ServiceSuccessfullyUpdatesTheMessage_ShouldNotifyGroupTheMessageIsUpdated() {
        // Arrange
        int messageId = 1;
        UserDto sessionUser = new() { UserId = 123, DisplayName = "", Email = "", Username = "" };
        UserDisplayDto sender = new() { UserId = 123 };
        MessageForUpdateDto messageForUpdate = CreateMessageForUpdateDto();
        MessageDto message = new() { Sender = sender };

        _mockService.Setup(x => x.AuthService.GetUserIdFromJwtToken(It.IsAny<string>()))
            .Returns(123);
        _mockService.Setup(x => x.MessageService.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(message);
        _mockService.Setup(x => x.MessageService.UpdateMessageAsync(It.IsAny<MessageForUpdateDto>()))
            .ReturnsAsync(message);

        // Act
        var result = await _controller.Update(messageId, messageForUpdate);

        // Assert
        _sendAsyncCalls.Should().Contain(call =>
            call.Item1 == "UpdateMessage" &&
            call.Item2.Length == 1);
    }

    [Fact]
    public async Task Update_ServiceSuccessfullyUpdatesTheMessage_ShouldReturnNoContent() {
        // Arrange
        int messageId = 1;
        UserDto sessionUser = new() { UserId = 123, DisplayName = "", Email = "", Username = "" };
        UserDisplayDto sender = new() { UserId = 123 };
        MessageForUpdateDto messageForUpdate = CreateMessageForUpdateDto();
        MessageDto message = new() { Sender = sender };

        _mockService.Setup(x => x.AuthService.GetUserIdFromJwtToken(It.IsAny<string>()))
            .Returns(123);
        _mockService.Setup(x => x.MessageService.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(message);
        _mockService.Setup(x => x.MessageService.UpdateMessageAsync(It.IsAny<MessageForUpdateDto>()))
            .ReturnsAsync(message);

        // Act
        var result = await _controller.Update(messageId, messageForUpdate) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeAssignableTo<MessageDto>();
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
    private static MessageForUpdateDto CreateMessageForUpdateDto() {
        return new MessageForUpdateDto {
            MessageId = 1,
            Content = "Updated content."
        };
    }

    private static ChatDto CreateP2PChatDto() {
        return new ChatDto {
            ChatId = 1,
            ChatTypeId = 1,
            ChatName = "Chat Name"
        };
    }

    private static ChatDto CreateGCChatDto() {
        return new ChatDto {
            ChatId = 1,
            ChatTypeId = 2,
            ChatName = "Chat Name"
        };
    }
}
