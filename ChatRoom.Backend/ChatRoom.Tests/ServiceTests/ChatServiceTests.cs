using AutoMapper;
using Contracts;
using Moq;
using RedisCacheService;
using Service.Contracts;
using Service;
using Microsoft.Extensions.Configuration;
using Entities.Models;
using ChatRoom.UnitTest.Helpers;
using Shared.DataTransferObjects.Chats;
using Entities.Exceptions;
using Shared.RequestFeatures;

namespace ChatRoom.UnitTest.ServiceTests
{
    public class ChatServiceTests
    {
        private readonly Mock<IRepositoryManager> _repositoryMock = new();
        private readonly Mock<ILoggerManager> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IRedisCacheManager> _cacheMock = new();
        private readonly Mock<IFileManager> _fileManagerMock = new();
        private readonly Mock<ISmtpClientManager> _smtpClientMock = new();

        private readonly IServiceManager _serviceManager;

        public ChatServiceTests()
        {
            _serviceManager = new ServiceManager(_repositoryMock.Object, _loggerMock.Object, _mapperMock.Object, _configurationMock.Object, _cacheMock.Object, _fileManagerMock.Object, _smtpClientMock.Object);
        }

        [Fact]
        public async Task CreateChatWithMembersAsync_ChatCreationFailed_ThrowChatNotCreatedException()
        {
            // arrange
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateValidGroupChatForCreationDto();
            Chat chatEntity = ChatDtoFactory.CreateChat(chatId:1, chatTypeId:chatForCreationDto.ChatTypeId);
            _mapperMock.Setup(m => m.Map<Chat>(It.IsAny<ChatForCreationDto>())).Returns(chatEntity).Verifiable();
            _repositoryMock.Setup(r => r.Chat.CreateChatAsync(It.IsAny<Chat>())).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<ChatNotCreatedException>(async () => await _serviceManager.ChatService.CreateChatWithMembersAsync(chatForCreationDto));

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            Assert.Equal($"The server failed to create chat room for users: {string.Join(",", chatForCreationDto.ChatMemberIds!)}. Try again later.", result.Message);
        }
         
        [Fact]
        public async Task CreateChatWithMembersAsync_InsertedChatMembersMismatch_ThrowsInsertedChatMemberRowsMismatchException()
        {
            // arrange
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateValidGroupChatForCreationDto(chatMemberIds: [1,2]);
            Chat chatEntity = ChatDtoFactory.CreateChat(chatId: 1, chatTypeId: chatForCreationDto.ChatTypeId);
            Chat createdChat = chatEntity;
            createdChat.Members = [];
            _mapperMock.Setup(m => m.Map<Chat>(It.IsAny<ChatForCreationDto>())).Returns(chatEntity).Verifiable();
            _repositoryMock.Setup(r => r.Chat.CreateChatAsync(It.IsAny<Chat>())).ReturnsAsync(createdChat).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.InsertChatMembers(It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync([]).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<InsertedChatMemberRowsMismatchException>(async () => await _serviceManager.ChatService.CreateChatWithMembersAsync(chatForCreationDto));

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            Assert.Equal($"There was an issue adding the chat members to the chatroom. Total inserted ids: {createdChat.Members.Count()}; Total expected ids to insert: {chatForCreationDto.ChatMemberIds!.Count()}.", result.Message);
        }

        [Fact]
        public async Task CreateChatWithMembersAsync_Success_ReturnsChatDto()
        {
            // arrange
            int chatId = 1;
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateValidGroupChatForCreationDto(chatMemberIds: [1, 2]);
            Chat chatEntity = ChatDtoFactory.CreateChat(chatId: chatId, chatTypeId: chatForCreationDto.ChatTypeId);
            Chat createdChat = chatEntity;
            createdChat.Members = [
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:1),
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:2)
                ];
            ChatDto chatToReturn = ChatDtoFactory.CreateGroupChatDto(chatId: chatId, members : [
                ChatMemberDtoFactory.CreateChatMemberDto(chatId:chatId,userId:1),
                ChatMemberDtoFactory.CreateChatMemberDto(chatId:chatId,userId:2)
                ]);
            _mapperMock.Setup(m => m.Map<Chat>(It.IsAny<ChatForCreationDto>())).Returns(chatEntity).Verifiable();
            _repositoryMock.Setup(r => r.Chat.CreateChatAsync(It.IsAny<Chat>())).ReturnsAsync(createdChat).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.InsertChatMembers(It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(createdChat.Members).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatDto>(It.IsAny<Chat>())).Returns(chatToReturn).Verifiable();

            // act
            var result = await _serviceManager.ChatService.CreateChatWithMembersAsync(chatForCreationDto);

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            Assert.IsType<ChatDto>(result);
            Assert.Equal(chatToReturn, result);
        }

        [Theory]
        [InlineData(1,0)]
        [InlineData(0,1)]
        public async Task CanViewAsync_ChatIdOrUserIdIsLessThanOne_ThrowsInvalidParameterException(int chatId, int userId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatService.CanViewAsync(chatId, userId));

            // assert
            Assert.Equal($"Invalid chatId : {chatId} and userId : {userId}", result.Message);
        }

        [Fact]

        public async Task CanViewAsync_UserIsNotAMember_ReturnsFalse()
        {
            // arrange
            int chatId = 1, userId = 1;
            IEnumerable<ChatMember> chatMembers = [
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:2),
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:3),
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:4)
                ];
            _repositoryMock.Setup(r => r.ChatMember.GetActiveChatMembersByChatIdAsync(chatId)).ReturnsAsync(chatMembers).Verifiable();

            // act
            var result = await _serviceManager.ChatService.CanViewAsync(chatId, userId);

            // assert
            _repositoryMock.Verify();
            Assert.IsType<bool>(result);
            Assert.False(result);
        }

        [Fact]

        public async Task CanViewAsync_UserIsAMember_ReturnsFalse()
        {
            // arrange
            int chatId = 1, userId = 1;
            IEnumerable<ChatMember> chatMembers = [
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:userId),
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:2),
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:3),
                ChatMemberDtoFactory.CreateChatMember(chatId:chatId,userId:4)
                ];
            _repositoryMock.Setup(r => r.ChatMember.GetActiveChatMembersByChatIdAsync(chatId)).ReturnsAsync(chatMembers).Verifiable();

            // act
            var result = await _serviceManager.ChatService.CanViewAsync(chatId, userId);

            // assert
            _repositoryMock.Verify();
            Assert.IsType<bool>(result);
            Assert.True(result);
        }

        [Theory]
        [InlineData(1,0)]
        [InlineData(0,1)]
        [InlineData(1,1)]
        public async Task GetP2PChatByUserIdsAsync_UserId1OrUserId2IsLessThanZeroOrUserIdIsSame_ThrowsInvalidParameterException(int userId1, int userId2)
        { 
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatService.GetP2PChatByUserIdsAsync(userId1, userId2));

            // assert
            Assert.Equal($"The user ids {userId1} and {userId2} is invalid.", result.Message);
        }

        [Fact]
        public async Task GetP2PChatByUserIdsAsync_GetP2PChatByUserIdsAsyncIsNull_ReturnsNull()
        {
            // arrange
            int userId1 = 1, userId2 = 2;
            _repositoryMock.Setup(r => r.Chat.GetP2PChatByUserIdsAsync(userId1, userId2)).Verifiable();

            // act
            var result = await _serviceManager.ChatService.GetP2PChatByUserIdsAsync(userId1, userId2);

            // assert
            _repositoryMock.Verify();
            Assert.Null(result);
        }

        [Fact]
        public async Task GetP2PChatByUserIdsAsync_GetP2PChatByUserIdsAsyncIsNotNull_ReturnsNull()
        {
            // arrange
            int userId1 = 1, userId2 = 2;
            Chat chat = ChatDtoFactory.CreateChat();
            ChatDto chatToReturn = ChatDtoFactory.CreateP2PChatDto();
            _repositoryMock.Setup(r => r.Chat.GetP2PChatByUserIdsAsync(userId1, userId2)).ReturnsAsync(chat).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatDto>(It.IsAny<Chat>())).Returns(chatToReturn).Verifiable();

            // act
            var result = await _serviceManager.ChatService.GetP2PChatByUserIdsAsync(userId1, userId2);

            // assert
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsType<ChatDto>(result);
            Assert.Equivalent(chatToReturn, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetChatByChatIdAsync_ChatIdIsLessthanZero_ThrowsInvalidParameterException(int chatId)
        {
            // act 
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatService.GetChatByChatIdAsync(chatId));

            // assert
            Assert.NotNull(result);
            Assert.Equal($"The chat id {chatId} is invalid.", result.Message);
        }

        [Fact]
        public async Task GetChatByChatIdAsync_ChatIsAvailableInCache_ReturnsChatDto()
        {
            // arrange
            int chatId = 1;
            Chat chat = ChatDtoFactory.CreateChat(chatId:chatId);
            ChatDto chatToReturn = ChatDtoFactory.CreateP2PChatDto(chatId:chatId);
            _cacheMock.Setup(c => c.GetCachedDataAsync<Chat>(It.IsAny<string>())).ReturnsAsync(chat).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatDto>(It.IsAny<Chat>())).Returns(chatToReturn).Verifiable();

            // act
            var result = await _serviceManager.ChatService.GetChatByChatIdAsync(chatId);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsType<ChatDto>(result);
            Assert.Equivalent(chatToReturn, result);
        }

        [Fact]
        public async Task GetChatByChatIdAsync_ChatNotFound_ThrowsChatNotFoundException()
        {
            // arrange
            int chatId = 1;
            _cacheMock.Setup(c => c.GetCachedDataAsync<Chat>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Chat.GetChatByChatIdAsync(chatId)).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<ChatNotFoundException>(async () => await _serviceManager.ChatService.GetChatByChatIdAsync(chatId));

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            Assert.NotNull(result);
            Assert.Equal($"The chat with id {chatId} does not exists in the database.", result.Message);
        }

        [Fact]
        public async Task GetChatByChatIdAsync_ChatIsFound_ReturnsChatDto()
        {
            // arrange
            int chatId = 1;
            Chat chat = ChatDtoFactory.CreateChat(chatId: chatId);
            ChatDto chatToReturn = ChatDtoFactory.CreateP2PChatDto(chatId: chatId);
            _cacheMock.Setup(c => c.GetCachedDataAsync<Chat>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.Chat.GetChatByChatIdAsync(chatId)).ReturnsAsync(chat).Verifiable();
            _cacheMock.Setup(c => c.SetCachedDataAsync(It.IsAny<string>(), It.IsAny<Chat>(), TimeSpan.FromMinutes(30))).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatDto>(It.IsAny<Chat>())).Returns(chatToReturn).Verifiable(Times.Once);

            // act
            var result = await _serviceManager.ChatService.GetChatByChatIdAsync(chatId);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            _repositoryMock.Verify();
            Assert.NotNull(result);
            Assert.IsType<ChatDto>(result);
            Assert.Equivalent(chatToReturn, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteChatAsync_ChatIdIsLessThanOne_ThrowsInvalidParameterException(int chatId)
        {
            // act 
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatService.DeleteChatAsync(chatId));

            // assert
            Assert.NotNull(result);
            Assert.Equal($"The chat id {chatId} is invalid.", result.Message);
        }

        [Fact]
        public async Task DeleteChatAsync_DeleteChatAsyncReturnsZero_ReturnsFalse()
        {
            // arrange
            int chatId = 1;
            _repositoryMock.Setup(r => r.Chat.DeleteChatAsync(chatId)).ReturnsAsync(0).Verifiable();

            // act
            var result = await _serviceManager.ChatService.DeleteChatAsync(chatId);

            // assert
            _repositoryMock.Verify();
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteChatAsync_DeleteChatAsyncReturnsMoreThanOne_ReturnsTrue()
        {
            // arrange
            int chatId = 1;
            _repositoryMock.Setup(r => r.Chat.DeleteChatAsync(chatId)).ReturnsAsync(1).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable();

            // act
            var result = await _serviceManager.ChatService.DeleteChatAsync(chatId);

            // assert
            _repositoryMock.Verify();
            Assert.True(result);
        }

        [Fact]
        public async Task GetChatListByChatIdAsync_Default_ReturnsChatDtoList()
        {
            // arrange
            ChatParameters chatParams = new () {
                Name = "Test",  
                PageNumber = 1,
                PageSize = 10,
                UserId = "1"
            };
            IEnumerable<Chat> userChats = [
                ChatDtoFactory.CreateChat(chatId:1),
                ChatDtoFactory.CreateChat(chatId:2),
                ChatDtoFactory.CreateChat(chatId:3),
                ];        
            IEnumerable<ChatDto> userChatDtos = [
                ChatDtoFactory.CreateP2PChatDto(chatId:1),
                ChatDtoFactory.CreateP2PChatDto(chatId:2),
                ChatDtoFactory.CreateP2PChatDto(chatId:3),
                ];
            _repositoryMock.Setup(r => r.Chat.SearchChatlistAsync(chatParams)).ReturnsAsync(userChats).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ChatDto>>(It.IsAny<IEnumerable<Chat>>())).Returns(userChatDtos).Verifiable();

            // act 
            var result = await _serviceManager.ChatService.GetChatListByChatIdAsync(chatParams);

            // arrange
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ChatDto>>(result);
        }

        [Fact]
        public async Task GetChatsByUserIdAsyncDefault_ReturnsChatDtoList()
        {
            // arrange
            int userId = 1;
            IEnumerable<Chat> userChats = [
                ChatDtoFactory.CreateChat(chatId:1),
                ChatDtoFactory.CreateChat(chatId:2),
                ChatDtoFactory.CreateChat(chatId:3),
                ];
            IEnumerable<ChatDto> userChatDtos = [
                ChatDtoFactory.CreateP2PChatDto(chatId:1),
                ChatDtoFactory.CreateP2PChatDto(chatId:2),
                ChatDtoFactory.CreateP2PChatDto(chatId:3),
                ];
            _repositoryMock.Setup(r => r.Chat.GetChatsByUserIdAsync(userId)).ReturnsAsync(userChats).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ChatDto>>(It.IsAny<IEnumerable<Chat>>())).Returns(userChatDtos).Verifiable();

            // act 
            var result = await _serviceManager.ChatService.GetChatsByUserIdAsync(userId);

            // arrange
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ChatDto>>(result);
        }

        [Fact]
        public async Task UpdateChatAsync_Default_NoOutput()
        {
            // arrange
            int chatId = 1;
            ChatForUpdateDto chat = new ChatForUpdateDto() { 
                DisplayPictureUrl = null
            };
            Chat chatEntity = ChatDtoFactory.CreateChat(chatId: chatId);
            _mapperMock.Setup(m => m.Map(It.IsAny<ChatForUpdateDto>(), It.IsAny<Chat>())).Verifiable();
            _repositoryMock.Setup(r => r.Chat.UpdateChatAsync(It.IsAny<Chat>())).ReturnsAsync(1).Verifiable();
            _repositoryMock.Setup(r => r.Chat.GetChatByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatEntity).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable();

            // act
            await _serviceManager.ChatService.UpdateChatAsync(chatId, chat);

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            _cacheMock.Verify();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task UpdateChatAsync_ChatIdLessThanOne_ThrowsInvalidParameterException(int chatId)
        {
            // arrange
            ChatForUpdateDto chat = new()
            {
                DisplayPictureUrl = null
            };

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatService.UpdateChatAsync(chatId, chat));

            // assert
            Assert.Equal($"The chat id {chatId} is invalid.", result.Message);
        }

        [Fact]
        public async Task UpdateChatAsync_GetChatByChatIdAsyncReturnsNull_ThrowsChatNotFoundException()
        {
            // arrange
            int chatId = 1;
            ChatForUpdateDto chat = new()
            {
                DisplayPictureUrl = null
            };
            Chat chatEntity = ChatDtoFactory.CreateChat(chatId: chatId);
            _repositoryMock.Setup(r => r.Chat.GetChatByChatIdAsync(It.IsAny<int>())).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<ChatNotFoundException>(async () =>await _serviceManager.ChatService.UpdateChatAsync(chatId, chat));

            // assert
            _repositoryMock.Verify();
        }

        [Fact]
        public async Task UpdateChatAsync_UpdateChatAsyncReturnsZero_ThrowsChatUpdateFailedException()
        {
            // arrange
            int chatId = 1;
            ChatForUpdateDto chat = new()
            {
                DisplayPictureUrl = null
            };
            Chat chatEntity = ChatDtoFactory.CreateChat(chatId: chatId);
            _mapperMock.Setup(m => m.Map(It.IsAny<ChatForUpdateDto>(), It.IsAny<Chat>())).Verifiable();
            _repositoryMock.Setup(r => r.Chat.GetChatByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatEntity).Verifiable();
            _repositoryMock.Setup(r => r.Chat.UpdateChatAsync(It.IsAny<Chat>())).ReturnsAsync(0).Verifiable();
            _loggerMock.Setup(l => l.LogError($"Failed to update the chat with id {chatEntity.ChatId}. " + $"Total rows affected: {0}. At {nameof(_serviceManager.ChatService)} - {nameof(_serviceManager.ChatService.UpdateChatAsync)}.")).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<ChatUpdateFailedException>(async () => await _serviceManager.ChatService.UpdateChatAsync(chatId, chat));

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            _loggerMock.Verify();
        }

        [Fact]
        public async Task UpdateChatAsync_HasNewDisplayImage_ThrowsChatNotFoundException()
        {
            // arrange
            int chatId = 1;
            ChatForUpdateDto chat = new()
            {
                DisplayPictureUrl = "test-picture"
            };
            Chat chatEntity = ChatDtoFactory.CreateChat(chatId: chatId, displayPictureUrl: "test-picture-updated");
            _mapperMock.Setup(m => m.Map(It.IsAny<ChatForUpdateDto>(), It.IsAny<Chat>())).Verifiable();
            _repositoryMock.Setup(r => r.Chat.UpdateChatAsync(It.IsAny<Chat>())).ReturnsAsync(1).Verifiable();
            _repositoryMock.Setup(r => r.Chat.GetChatByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatEntity).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable();
            _fileManagerMock.Setup(f => f.DeleteImageAsync(It.IsAny<string>())).Verifiable();

            // act
            await _serviceManager.ChatService.UpdateChatAsync(chatId, chat);

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            _cacheMock.Verify();
            _fileManagerMock.Verify();
        }
    }
}
