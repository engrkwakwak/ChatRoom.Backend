using AutoMapper;
using ChatRoom.Backend;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using FluentAssertions;
using MongoDB.Bson.Serialization.Conventions;
using Moq;
using RedisCacheService;
using Service;
using Shared.DataTransferObjects.Messages;
using Shared.RequestFeatures;

namespace ChatRoom.UnitTest.ServiceTests;
public class MessageServiceTests {
    private readonly Mock<IRepositoryManager> _mockRepo;
    private readonly MessageService _service;

    public MessageServiceTests() {
        _mockRepo = new Mock<IRepositoryManager>();

        var mappingConfig = new MapperConfiguration(mc => {
            mc.AddProfile(new MappingProfile());
        });
        var mapper = mappingConfig.CreateMapper();

        _service = new MessageService(_mockRepo.Object, mapper);
    }

    [Fact]
    public async Task GetMessagesByChatIdAsync_RepositoryReturnsEmptyList_ShouldReturnEmptyListOfMessagesWithMetaData() {
        // Arrange
        PagedList<Message> messagesWithMetaData = new([], 0, 1, 10);
        MessageParameters messageParameters = new();
        int chatId = 1;

        _mockRepo.Setup(x => x.Message.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync(messagesWithMetaData);

        // Act
        var (messages, metaData) = await _service.GetMessagesByChatIdAsync(messageParameters, chatId);

        // Assert
        messages.Should().NotBeNull();
        messages.Should().BeEmpty();
        
        metaData.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMessagesByChatIdAsync_ValidMessageParameters_ShouldReturnMessagesWithMetaData() {
        // Arrange
        PagedList<Message> messagesWithMetaData = new(CreateMessages(), 0, 1, 10);
        MessageParameters messageParameters = new();
        int chatId = 1;

        _mockRepo.Setup(x => x.Message.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync(messagesWithMetaData);

        // Act
        var (messages, metaData) = await _service.GetMessagesByChatIdAsync(messageParameters, chatId);

        // Assert
        messages.Should().NotBeNull();
        messages.Should().HaveCount(messagesWithMetaData.Count);

        metaData.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMessagesByChatIdAsync_ValidMessageParameters_ShouldMapMessagesToMessageDtosCorrectly() {
        // Arrange
        PagedList<Message> messagesWithMetaData = new(CreateMessages(), 0, 1, 10);
        MessageParameters messageParameters = new();
        int chatId = 1;

        _mockRepo.Setup(x => x.Message.GetMessagesByChatIdAsync(It.IsAny<MessageParameters>(), It.IsAny<int>()))
            .ReturnsAsync(messagesWithMetaData);

        // Act
        var (messages, metaData) = await _service.GetMessagesByChatIdAsync(messageParameters, chatId);

        // Assert
        messages.Should().BeEquivalentTo(messagesWithMetaData, options => options
            .Excluding(m => m.Chat)
            .Excluding(m => m.User)
            .Excluding(m => m.MessageType)
            .Excluding(m => m.Status)
            .Excluding(m => m.LastSeenUsers)
            .Excluding(m => m.DateUpdated)
            .Excluding(m => m.StatusId)
        );
    }

    [Fact]
    public async Task InsertMessageAsync_ValidMessageForCreateDto_ShouldMapMessageForCreationDtoToMessageCorrectly() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        Message msgEntity = CreateMessage();
        Message? capturedMsgArgument = null;

        _mockRepo.Setup(x => x.Message.InsertMessageAsync(It.IsAny<Message>()))
            .Callback<Message>(msg => capturedMsgArgument = msg)
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.InsertMessageAsync(messageToSend);

        // Assert
        capturedMsgArgument.Should().NotBeNull();
        capturedMsgArgument?.ChatId.Should().Be(messageToSend.ChatId);
        capturedMsgArgument?.Content.Should().Be(messageToSend.Content);
        capturedMsgArgument?.MsgTypeId.Should().Be(messageToSend.MsgTypeId);
        capturedMsgArgument?.SenderId.Should().Be(messageToSend.SenderId);
    }

    [Fact]
    public async Task InsertMessageAsync_MessageIsNotInserted_ShouldThrowException() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.InsertMessageAsync(It.IsAny<Message>()))
            .ReturnsAsync((Message?)null);

        // Act
        Func<Task> act = async() => await _service.InsertMessageAsync(messageToSend);

        // Assert
        await act.Should().ThrowAsync<MessageNotCreatedException>()
            .WithMessage("Something went wrong while sending the message. Please try again later.");
    }

    [Fact]
    public async Task InsertMessageAsync_MessageIsInsertedSuccessfully_ShouldReturnMessageDto() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.InsertMessageAsync(It.IsAny<Message>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.InsertMessageAsync(messageToSend);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<MessageDto>();
    }

    [Fact]
    public async Task InsertMessagAsync_MessageIsInsertedSuccessfully_ShouldMapMessageToMessageDtoSuccessfully() {
        // Arrange
        MessageForCreationDto messageToSend = CreateMessageForCreationDto();
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.InsertMessageAsync(It.IsAny<Message>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.InsertMessageAsync(messageToSend);

        // Assert
        result.ChatId.Should().Be(msgEntity.ChatId);
        result.Content.Should().Be(msgEntity.Content);
        result.MsgTypeId.Should().Be(msgEntity.MsgTypeId);
        result.SenderId.Should().Be(msgEntity.SenderId);
    }

    [Fact]
    public async Task DeleteMessageAsync_MessageDoesNotExistInDatabase_ShouldThrowNotFoundException() {
        // Arrange
        int messageId = 1;

        _mockRepo.Setup(x => x.Message.DeleteMessageAsync(It.IsAny<int>()))
            .ThrowsAsync(new MessageNotFoundException(messageId));

        // Act
        Func<Task> act = async() => await _service.DeleteMessageAsync(messageId);

        // Assert
        await act.Should().ThrowAsync<MessageNotFoundException>()
            .WithMessage($"The message with id {messageId} does not exist in the database.");
    }

    [Fact]
    public async Task DeleteMessageAsync_RepositoryFailedToDeleteTheMessage_ShouldThrowMessageUpdateFailedException() {
        // Arrange
        int messageId = 1;
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.DeleteMessageAsync(It.IsAny<int>()))
            .ReturnsAsync(0);
        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        Func<Task> act = async () => await _service.DeleteMessageAsync(messageId);

        // Assert
        await act.Should().ThrowAsync<MessageUpdateFailedException>()
            .WithMessage("Something went wrong while deleting the message. Please try again later.");
    }

    [Fact]
    public async Task DeleteMessageAsync_RepositorySuccessfullyDeletesTheMessage_ShouldReturnDeletedMessage() {
        // Arrange
        int messageId = 1;
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.DeleteMessageAsync(It.IsAny<int>()))
            .ReturnsAsync(1);
        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result =  await _service.DeleteMessageAsync(messageId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<MessageDto>();
    }

    [Fact]
    public async Task DeleteMessageAsync_RepositorySuccessfullyDeletesTheMessage_ShouldMapMessageToMessageDtoSuccessfully() {
        // Arrange
        int messageId = 1;
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.DeleteMessageAsync(It.IsAny<int>()))
            .ReturnsAsync(1);
        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.DeleteMessageAsync(messageId);

        // Assert
        result.ChatId.Should().Be(msgEntity.ChatId);
        result.Content.Should().Be(msgEntity.Content);
        result.MsgTypeId.Should().Be(msgEntity.MsgTypeId);
        result.SenderId.Should().Be(msgEntity.SenderId);
    }

    [Fact]
    public async Task GetMessageByMessageIdAsync_MessageDoesNotExistInDatabase_ShouldThrowNotFoundException() {
        // Arrange
        int messageId = 1;

        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new MessageNotFoundException(messageId));

        // Act
        Func<Task> act = async () => await _service.GetMessageByMessageIdAsync(messageId);

        // Assert
        await act.Should().ThrowAsync<MessageNotFoundException>()
            .WithMessage($"The message with id {messageId} does not exist in the database.");
    }

    [Fact]
    public async Task GetMessageByMessageIdAsync_MessageExistsInTheDatabase_ShouldReturnMessageDto() {
        // Arrange
        int messageId = 1;
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.GetMessageByMessageIdAsync(messageId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<MessageDto>();
    }

    [Fact]
    public async Task GetMessageByMessageIdAsync_MessageExistsInTheDatabase_ShouldMapMessageToMessageDtoSuccessfully() {
        // Arrange
        int messageId = 1;
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.GetMessageByMessageIdAsync(messageId);

        // Assert
        result.ChatId.Should().Be(msgEntity.ChatId);
        result.Content.Should().Be(msgEntity.Content);
        result.MsgTypeId.Should().Be(msgEntity.MsgTypeId);
        result.SenderId.Should().Be(msgEntity.SenderId);
    }

    [Fact]
    public async Task UpdateMessage_MessageDoesNotExistInTheDatabase_ShouldThrowNotFoundException() {
        // Arrange
        int messageId = 1;
        MessageForUpdateDto msgForUpd = CreateMessageForUpdateDto();

        _mockRepo.Setup(x => x.Message.UpdateMessageAsync(It.IsAny<Message>()))
            .ThrowsAsync(new MessageNotFoundException(messageId));

        // Act
        Func<Task> act = async () => await _service.UpdateMessageAsync(msgForUpd);

        // Assert
        await act.Should().ThrowAsync<MessageNotFoundException>()
            .WithMessage($"The message with id {messageId} does not exist in the database.");
    }

    [Fact]
    public async Task UpdateMessage_RepositoryFailedToUpdateTheMessage_ShouldThrowUpdateFailedException() {
        // Arrange
        MessageForUpdateDto msgForUpd = CreateMessageForUpdateDto();
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.UpdateMessageAsync(It.IsAny<Message>()))
            .ReturnsAsync(0);
        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        Func<Task> act = async () => await _service.UpdateMessageAsync(msgForUpd);

        // Assert
        await act.Should().ThrowAsync<MessageUpdateFailedException>()
            .WithMessage("Something went wrong while updating the message. Please try again later.");
    }

    [Fact]
    public async Task UpdateMessage_MessageExistsInTheDatabase_ShouldMapMessageForUpdateToMessageCorrectly() {
        // Arrange
        MessageForUpdateDto msgForUpd = CreateMessageForUpdateDto();
        Message msgEntity = CreateMessage();
        Message? capturedMsgArgument = null;

        _mockRepo.Setup(x => x.Message.UpdateMessageAsync(It.IsAny<Message>()))
            .Callback<Message>(msg => capturedMsgArgument = msg)
            .ReturnsAsync(1);
        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.UpdateMessageAsync(msgForUpd);

        // Assert
        capturedMsgArgument.Should().NotBeNull();
        capturedMsgArgument!.MessageId.Should().Be(msgForUpd.MessageId);
        capturedMsgArgument!.Content.Should().Be(msgForUpd.Content);
    }

    [Fact]
    public async Task UpdateMessage_RepositorySuccessfullyUpdatesTheMessage_ShouldReturnMessageDto() {
        // Arrange
        MessageForUpdateDto msgForUpd = CreateMessageForUpdateDto();
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.UpdateMessageAsync(It.IsAny<Message>()))
            .ReturnsAsync(1);
        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.UpdateMessageAsync(msgForUpd);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<MessageDto>();
    }

    [Fact]
    public async Task UpdateMessage_RepositorySuccessfullyUpdatesTheMessage_ShouldMapMessageToMessageDtoSuccessfully() {
        // Arrange
        MessageForUpdateDto msgForUpd = CreateMessageForUpdateDto();
        Message msgEntity = CreateMessage();

        _mockRepo.Setup(x => x.Message.UpdateMessageAsync(It.IsAny<Message>()))
            .ReturnsAsync(1);
        _mockRepo.Setup(x => x.Message.GetMessageByMessageIdAsync(It.IsAny<int>()))
            .ReturnsAsync(msgEntity);

        // Act
        var result = await _service.UpdateMessageAsync(msgForUpd);

        // Assert
        result.ChatId.Should().Be(msgEntity.ChatId);
        result.Content.Should().Be(msgEntity.Content);
        result.MsgTypeId.Should().Be(msgEntity.MsgTypeId);
        result.SenderId.Should().Be(msgEntity.SenderId);
    }

    private static List<Message> CreateMessages() {
        return [
            new Message {
                ChatId = 1,
                Content = "Message 1",
                MessageId = 1,
                SenderId = 1,
            },
            new Message {
                ChatId = 1,
                Content = "Message 2",
                MessageId = 2,
                SenderId = 2,
            },
            new Message {
                ChatId = 1,
                Content = "Message 3",
                MessageId = 3,
                SenderId = 1,
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

    private static Message CreateMessage() {
        return new Message {
            ChatId = 1,
            Content = "Content",
            MsgTypeId = 1,
            SenderId = 1
        };
    }

    private static MessageForUpdateDto CreateMessageForUpdateDto() {
        return new MessageForUpdateDto {
            MessageId = 1,
            Content = "Modified Content"
        };
    }
}
