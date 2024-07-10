using AutoMapper;
using Contracts;
using Moq;
using RedisCacheService;
using Service.Contracts;
using Service;
using Microsoft.Extensions.Configuration;
using Shared.DataTransferObjects.Chats;
using ChatRoom.UnitTest.Helpers;
using Entities.Exceptions;
using Entities.Models;
using ZstdSharp.Unsafe;
using Shared.DataTransferObjects.Users;
using System.Security.Cryptography.Xml;

namespace ChatRoom.UnitTest.ServiceTests
{
    public class ChatMemberServiceTests
    {
        private readonly Mock<IRepositoryManager> _repositoryMock = new();
        private readonly Mock<ILoggerManager> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IRedisCacheManager> _cacheMock = new();
        private readonly Mock<IFileManager> _fileManagerMock = new();

        private readonly IServiceManager _serviceManager;

        public ChatMemberServiceTests()
        {
            _serviceManager = new ServiceManager(_repositoryMock.Object, _loggerMock.Object, _mapperMock.Object, _configurationMock.Object, _cacheMock.Object, _fileManagerMock.Object);
        }

        [Fact]
        public async Task UpdateLastSeenMessageAsync_GetChatMemberByChatIdAndUserIdFailed_ThrowsChatMemberNotFoundException()
        {
            // arrange
            int chatId = 1, userId = 1;
            ChatMemberForUpdateDto chatMemberForUpdateDto = ChatMemberDtoFactory.CreateChatMemberForUpdateDto();
            _repositoryMock.Setup(rm => rm.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId)).Verifiable();

            // act 
            var result = await Assert.ThrowsAsync<ChatMemberNotFoundException>(
                async () => await _serviceManager.ChatMemberService.UpdateLastSeenMessageAsync(chatId, userId, chatMemberForUpdateDto)
                );

            // assert
            _repositoryMock.Verify();
            Assert.Equal($"The chat with id {chatId} does not contain a member with id {userId}.", result.Message);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        public async Task UpdateLastSeenMessageAsync_ChatIdOrUserIdIsLessThanOne_ThrowsInvalidParameterException(int chatId, int userId)
        {
            // arrange
            ChatMemberForUpdateDto chatMemberForUpdateDto = ChatMemberDtoFactory.CreateChatMemberForUpdateDto();

            // act 
            var result = await Assert.ThrowsAsync<InvalidParameterException>(
                async () => await _serviceManager.ChatMemberService.UpdateLastSeenMessageAsync(chatId, userId, chatMemberForUpdateDto)
            );

            // assert
            _repositoryMock.Verify();
            Assert.Equal($"Invalid chatId : {chatId} or userId : {userId}", result.Message);
        }

        [Fact]
        public async Task UpdateLastSeenMessageAsync_LastSaveSeenMessageIsMoreRecent_ReturnsChatMemberDto()
        {
            // arrange
            int chatId = 1, userId = 1;
            ChatMemberForUpdateDto chatMemberForUpdateDto = ChatMemberDtoFactory.CreateChatMemberForUpdateDto(lastSeenMessageId: 1);
            ChatMember chatMemberEntity = ChatMemberDtoFactory.CreateChatMember(lastSeenMessageId: 5);
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(isAdmin: chatMemberEntity.IsAdmin, userId: chatMemberEntity.UserId);
            _repositoryMock.Setup(rm => rm.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId)).ReturnsAsync(chatMemberEntity).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatMemberDto>(chatMemberEntity)).Returns(chatMemberDto);

            // act 
            var result = await _serviceManager.ChatMemberService.UpdateLastSeenMessageAsync(chatId, userId, chatMemberForUpdateDto);

            // assert
            _repositoryMock.Verify();
            _mapperMock.Verify(m => m.Map<ChatMemberDto>(chatMemberEntity));
            Assert.IsType<ChatMemberDto>(result);
            Assert.Equivalent(chatMemberDto, result);
        }

        [Fact]
        public async Task UpdateLastSeenMessageAsync_UpdateLastSeenMessagedAsyncFailed_ThrowsLastSeenMessageUpdateFailedException()
        {
            // arrange
            int chatId = 1, userId = 1;
            ChatMemberForUpdateDto chatMemberForUpdateDto = ChatMemberDtoFactory.CreateChatMemberForUpdateDto(lastSeenMessageId: 5);
            ChatMember chatMemberEntity = ChatMemberDtoFactory.CreateChatMember(lastSeenMessageId: 1);
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(isAdmin: chatMemberEntity.IsAdmin, userId: chatMemberEntity.UserId);
            _repositoryMock.Setup(rm => rm.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId)).ReturnsAsync(chatMemberEntity).Verifiable();
            _repositoryMock.Setup(rm => rm.ChatMember.UpdateLastSeenMessageAsync(It.IsAny<ChatMember>())).Verifiable();

            // act 
            var result = await Assert.ThrowsAsync<LastSeenMessageUpdateFailedException>( 
                async () => await _serviceManager.ChatMemberService.UpdateLastSeenMessageAsync(chatId, userId, chatMemberForUpdateDto)
                );

            // assert
            _repositoryMock.Verify();
            _mapperMock.Verify(m => m.Map(It.IsAny<ChatMemberForUpdateDto>(), It.IsAny<ChatMember>()));
            Assert.Equal($"The server failed to update the last seen message of user with id {userId} in chat with id {chatId}.", result.Message);
        }

        [Fact]
        public async Task UpdateLastSeenMessageAsync_UpdateLastSeenMessagedAsyncSucceeds_ReturnsChatMemberDto()
        {
            // arrange
            int chatId = 1, userId = 1;
            ChatMemberForUpdateDto chatMemberForUpdateDto = ChatMemberDtoFactory.CreateChatMemberForUpdateDto(lastSeenMessageId: 5);
            ChatMember chatMemberEntity = ChatMemberDtoFactory.CreateChatMember(lastSeenMessageId: 1);
            ChatMember updatedChatMember = ChatMemberDtoFactory.CreateChatMember(lastSeenMessageId: 5);
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(isAdmin: chatMemberEntity.IsAdmin, userId: chatMemberEntity.UserId);
            ChatMemberDto chatMemberToReturn = ChatMemberDtoFactory.CreateChatMemberDto(isAdmin: updatedChatMember.IsAdmin, userId: updatedChatMember.UserId);
            _repositoryMock.Setup(rm => rm.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId)).ReturnsAsync(chatMemberEntity).Verifiable();
            _repositoryMock.Setup(rm => rm.ChatMember.UpdateLastSeenMessageAsync(It.IsAny<ChatMember>())).ReturnsAsync(updatedChatMember).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatMemberDto>(updatedChatMember)).Returns(chatMemberToReturn).Verifiable();

            // act 
            var result = await _serviceManager.ChatMemberService.UpdateLastSeenMessageAsync(chatId, userId, chatMemberForUpdateDto);

            // assert
            _repositoryMock.Verify();
            _mapperMock.Verify(m => m.Map(It.IsAny<ChatMemberForUpdateDto>(), It.IsAny<ChatMember>()), Times.Once);
            _cacheMock.Verify(c => c.RemoveDataAsync(It.IsAny<string>()), Times.Once);
            Assert.IsType<ChatMemberDto>(result);
            Assert.Equivalent(chatMemberToReturn , result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetActiveChatMembersByChatIdAsync_ChatIdIsLessThanOne_ThrowsInvalidParameterException(int chatId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId));

            // assert
            Assert.Equal($"Invalid chatId : {chatId}.", result.Message);
        }

        [Fact]
        public async Task GetActiveChatMembersByChatIdAsync_DataIsAvailableInCache_ReturnsChatMemberDto()
        {
            // arrange
            int chatId = 1;
            IEnumerable<ChatMember> chatMembers = [
                ChatMemberDtoFactory.CreateChatMember(chatId: 1, isAdmin: true, userId:1),
                ChatMemberDtoFactory.CreateChatMember(chatId: 1, isAdmin: false, userId:2),
                ChatMemberDtoFactory.CreateChatMember(chatId: 1, isAdmin: false, userId:3),
                ];
            IEnumerable<ChatMemberDto> chatMemberDtos = [
                ChatMemberDtoFactory.CreateChatMemberDto(chatId: 1, isAdmin: true, userId:1),
                ChatMemberDtoFactory.CreateChatMemberDto(chatId: 1, isAdmin: false, userId:2),
                ChatMemberDtoFactory.CreateChatMemberDto(chatId: 1, isAdmin: false, userId:3),
                ];
            _cacheMock.Setup(cm => cm.GetCachedDataAsync<IEnumerable<ChatMember>>(It.IsAny<string>())).ReturnsAsync(chatMembers).Verifiable(Times.Once);
            _mapperMock.Setup(m => m.Map<IEnumerable<ChatMemberDto>>(It.IsAny<IEnumerable<ChatMember>>())).Returns(chatMemberDtos).Verifiable(Times.Once);

            // act
            var result = await _serviceManager.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.IsAssignableFrom<IEnumerable<ChatMemberDto>>(result);
            Assert.Equivalent(chatMemberDtos, result);
        }

        [Fact]
        public async Task GetActiveChatMembersByChatIdAsync_DataIsNotAvailableOnCache_ReturnsChatMemberDto()
        {
            // arrange
            int chatId = 1;
            IEnumerable<ChatMember> chatMembers = [
                ChatMemberDtoFactory.CreateChatMember(chatId: 1, isAdmin: true, userId:1),
                ChatMemberDtoFactory.CreateChatMember(chatId: 1, isAdmin: false, userId:2),
                ChatMemberDtoFactory.CreateChatMember(chatId: 1, isAdmin: false, userId:3),
                ];
            IEnumerable<ChatMemberDto> chatMemberDtos = [
                ChatMemberDtoFactory.CreateChatMemberDto(chatId: 1, isAdmin: true, userId:1),
                ChatMemberDtoFactory.CreateChatMemberDto(chatId: 1, isAdmin: false, userId:2),
                ChatMemberDtoFactory.CreateChatMemberDto(chatId: 1, isAdmin: false, userId:3),
                ];
            _cacheMock.Setup(cm => cm.GetCachedDataAsync<IEnumerable<ChatMember>>(It.IsAny<string>())).Verifiable();
            _mapperMock.Setup(m => m.Map<IEnumerable<ChatMemberDto>>(It.IsAny<IEnumerable<ChatMember>>())).Returns(chatMemberDtos).Verifiable(Times.Once);
            _repositoryMock.Setup(r => r.ChatMember.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();

            // act
            var result = await _serviceManager.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            _cacheMock.Verify(cm => cm.SetCachedDataAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ChatMember>>(), TimeSpan.FromMinutes(30)));
            Assert.IsAssignableFrom<IEnumerable<ChatMemberDto>>(result);
            Assert.Equivalent(chatMemberDtos, result);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task GetChatMemberByChatIdUserIdAsync_ChatIdOrUserIdIsLessThanOne_ThrowsInvalidParameterException(int chatId, int userId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () =>  await _serviceManager.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId));

            // assert
            Assert.Equal($"Invalid chatId : {chatId} or userId : {userId}", result.Message);
        }

        [Fact]
        public async Task GetChatMemberByChatIdUserIdAsync_DataIsAvailableInCache_ReturnsChatMemberDto()
        {
            int chatId=1, userId=1;
            ChatMember chatMember = ChatMemberDtoFactory.CreateChatMember(userId: userId, chatId:chatId);
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(userId: userId, chatId:chatId);
            _cacheMock.Setup(c => c.GetCachedDataAsync<ChatMember>(It.IsAny<string>())).ReturnsAsync(chatMember).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatMemberDto>(It.IsAny<ChatMember>())).Returns(chatMemberDto).Verifiable(Times.Once);

            // act
            var result = await _serviceManager.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId);

            // assert
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.IsType<ChatMemberDto>(result);
            Assert.Equivalent(chatMemberDto, result);
        }

        [Fact]
        public async Task GetChatMemberByChatIdUserIdAsync_DataIsNotAvailableInCache_ReturnsChatMemberDto()
        {
            int chatId = 1, userId = 1;
            ChatMember chatMember = ChatMemberDtoFactory.CreateChatMember(userId: userId, chatId: chatId);
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(userId: userId, chatId: chatId);
            _cacheMock.Setup(c => c.GetCachedDataAsync<ChatMember>(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.GetChatMemberByChatIdAndUserIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(chatMember).Verifiable();
            _cacheMock.Setup(c => c.SetCachedDataAsync(It.IsAny<string>(), It.IsAny<ChatMember>(), TimeSpan.FromMinutes(30))).Verifiable(Times.Once);
            _mapperMock.Setup(m => m.Map<ChatMemberDto>(It.IsAny<ChatMember>())).Returns(chatMemberDto).Verifiable(Times.Once);

            // act
            var result = await _serviceManager.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            _mapperMock.Verify();
            Assert.IsType<ChatMemberDto>(result);
            Assert.Equivalent(chatMemberDto, result);
        }

        [Theory]
        [InlineData(0,1)]
        [InlineData(1,0)]
        public async Task SetIsAdminAsync_ChatIdOrUserIdIsLessThanOne_ThrowsInvalidParameterException(int chatId, int userId)
        {
            // arrange
            bool isAdmin = false;

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatMemberService.SetIsAdminAsync(chatId, userId, isAdmin));

            // assert
            Assert.Equal($"Invalid chatId : {chatId} or userId : {userId}", result.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SetIsAdminAsync_SetIsAdminAsyncIsLessThanOne_ReturnsFalse(int affectedRows)
        {
            // arrange
            int chatId = 1, userId = 1;
            bool isAdmin = false;
            _repositoryMock.Setup(r => r.ChatMember.SetIsAdminAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(affectedRows).Verifiable(Times.Once);
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable(Times.Exactly(2));

            // act
            var result = await _serviceManager.ChatMemberService.SetIsAdminAsync(chatId , userId, isAdmin);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            Assert.False(result);
        }

        [Fact]
        public async Task SetIsAdminAsync_SetIsAdminAsyncOne_ReturnsTrue()
        {
            // arrange
            int chatId = 1, userId = 1;
            bool isAdmin = false;
            _repositoryMock.Setup(r => r.ChatMember.SetIsAdminAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(1).Verifiable(Times.Once);
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable(Times.Exactly(2));

            // act
            var result = await _serviceManager.ChatMemberService.SetIsAdminAsync(chatId, userId, isAdmin);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            Assert.True(result);
        }

        [Theory]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 0)]
        [InlineData(1, 1, 4)]
        public async Task SetChatMemberStatus_EitherUserIdStatusIdOrChatIdIsInvalid_ThrowsInvalidParameterException(int chatId, int userId, int statusId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatMemberService.SetChatMemberStatus(chatId, userId, statusId));

            // assert
            Assert.Equal($"Invalid chatId : {chatId} or userId : {userId} or statusId: {statusId}", result.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SetChatMemberStatus_SetChatMemberStatusIsLessThanOne_ReturnsFalse(int affectedRows)
        {
            // arrange
            int chatId = 1, userId = 1, statusId=1;
            _repositoryMock.Setup(r => r.ChatMember.SetChatMemberStatus(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(affectedRows).Verifiable(Times.Once);
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable(Times.Exactly(2));

            // act
            var result = await _serviceManager.ChatMemberService.SetChatMemberStatus(chatId, userId, statusId);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            Assert.False(result);
        }

        [Fact]
        public async Task SetChatMemberStatus_SetChatMemberStatusIsOne_ReturnsTrue()
        {
            // arrange
            int chatId = 1, userId = 1, statusId = 1;
            _repositoryMock.Setup(r => r.ChatMember.SetChatMemberStatus(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(1).Verifiable(Times.Once);
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable(Times.Exactly(2));

            // act
            var result = await _serviceManager.ChatMemberService.SetChatMemberStatus(chatId, userId, statusId);

            // assert
            _cacheMock.Verify();
            _repositoryMock.Verify();
            Assert.True(result);
        }

        // InsertChatMembersAsync_ChatIdIsInvalid_ThrowsInvalidParameterException
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task InsertChatMembersAsync_ChatIdIsInvalid_ThrowsInvalidParameterException(int chatId)
        {
            // arrange
            IEnumerable<int> userIds = [1,2, 3];    

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatMemberService.InsertChatMembersAsync(chatId, userIds));

            // assert
            Assert.Equal($"Invalid chatId : {chatId}", result.Message);
        }


        // InsertChatMembersAsync_UserIdsHasInvalidItem_ThrowsInvalidParameterException
        [Fact]
        public async Task InsertChatMembersAsync_UserIdsHasInvalidItem_ThrowsInvalidParameterException()
        {
            // arrange
            int chatId = 1;
            IEnumerable<int> userIds = [1, 2, 3, 0];

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatMemberService.InsertChatMembersAsync(chatId, userIds));

            // assert
            Assert.Equal("Invalid parameter user id list. An invalid member is detected.", result.Message);
        }

        // InsertChatMembersAsync_ChatMembersInsertedSuccess_ReturnsChatMemberList
        [Fact]
        public async Task InsertChatMembersAsync_ChatMembersInsertedSuccess_ReturnsChatMemberList()
        {
            // arrange
            int chatId = 1;
            IEnumerable<int> userIds = [1, 2, 3, 4];
            IEnumerable<ChatMember> chatMembers = [
                   ChatMemberDtoFactory.CreateChatMember(userId: 1),
                   ChatMemberDtoFactory.CreateChatMember(userId: 2),
                   ChatMemberDtoFactory.CreateChatMember(userId: 3),
                   ChatMemberDtoFactory.CreateChatMember(userId: 4)
                ];
           ChatMemberDto[] chatMemberDtos = [
                   ChatMemberDtoFactory.CreateChatMemberDto(userId: 1),
                   ChatMemberDtoFactory.CreateChatMemberDto(userId: 2),
                   ChatMemberDtoFactory.CreateChatMemberDto(userId: 3),
                   ChatMemberDtoFactory.CreateChatMemberDto(userId: 4)
                ];
            _repositoryMock.Setup(r => r.ChatMember.InsertChatMembers(It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(chatMembers).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatMemberDto[]>(It.IsAny<IEnumerable<ChatMember>>())).Returns(chatMemberDtos).Verifiable();


            // act
            var result = await _serviceManager.ChatMemberService.InsertChatMembersAsync(chatId, userIds);

            // assert
            _repositoryMock.Verify();
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.IsAssignableFrom<ChatMemberDto[]>(result);
            Assert.Equivalent(chatMemberDtos, result);
        }

        // InsertChatMemberAsync_ChatIdOrUserIdIsLessThanOne_ThrowsInvalidParameterException
        [Theory]
        [InlineData(0,1)]
        [InlineData(1,0)]
        public async Task InsertChatMemberAsync_ChatIdOrUserIdIsLessThanOne_ThrowsInvalidParameterException(int chatId, int userId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _serviceManager.ChatMemberService.InsertChatMemberAsync(chatId, userId));

            // assert
            Assert.Equal($"Invalid chat id : {chatId} and user id : {userId}.", result.Message);
        }

        [Fact]
        public async Task InsertChatMemberAsync_ChatMemberIsNull_ReturnChatMemberDto()
        {
            // arrange 
            int chatId = 1, userId = 1;
            User user = UserDtoFactory.CreateUser(userId: userId);
            IEnumerable<ChatMember> chatMembers = [
                    ChatMemberDtoFactory.CreateChatMember(chatId: chatId, userId: userId)
                ];
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(userId: userId, chatId: chatId);
            _repositoryMock.Setup(r => r.User.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.GetChatMemberByChatIdAndUserIdAsync(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.InsertChatMembers(chatId, It.IsAny<IEnumerable<int>>())).ReturnsAsync(chatMembers).Verifiable();
            _mapperMock.Setup(m => m.Map<ChatMemberDto>(It.IsAny<ChatMember>())).Returns(chatMemberDto).Verifiable();

            // act
            var result = await _serviceManager.ChatMemberService.InsertChatMemberAsync(chatId, userId);

            // assert
            _repositoryMock.Verify();
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.IsType<ChatMemberDto>(result);
            Assert.Equivalent(chatMemberDto ,result);

        }

        // InsertChatMemberAsync_UserIsNull_ReturnChatMemberDto
        [Fact]
        public async Task InsertChatMemberAsync_UserIsNull_ThrowsUserNotFoundException()
        {
            // arrange 
            int chatId = 1, userId = 1;
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(userId: userId, chatId: chatId);
            _repositoryMock.Setup(r => r.User.GetUserByIdAsync(It.IsAny<int>())).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<UserIdNotFoundException>(async () => await _serviceManager.ChatMemberService.InsertChatMemberAsync(chatId, userId));

            // assert
            _repositoryMock.Verify();
            Assert.Equal($"The user with id: {userId} doesn't exists in the database.", result.Message);
        }

        // InsertChatMemberAsync_SetChatMemberStatusIsLessThanOne_ThrowsException
        [Fact]
        public async Task InsertChatMemberAsync_SetChatMemberStatusIsLessThanOne_ThrowsException()
        {
            // arrange 
            int chatId = 1, userId = 1;
            ChatMember member = ChatMemberDtoFactory.CreateChatMember(chatId: chatId, userId: userId);
            User user = UserDtoFactory.CreateUser(userId: userId);
            IEnumerable<ChatMember> chatMembers = [member];
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(userId: userId, chatId: chatId);
            _repositoryMock.Setup(r => r.User.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.GetChatMemberByChatIdAndUserIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(member).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.SetChatMemberStatus(It.IsAny<int>(), It.IsAny<int>(), 1)).ReturnsAsync(0).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<ChatMemberNotUpdatedException>(async () => await _serviceManager.ChatMemberService.InsertChatMemberAsync(chatId, userId));

            // assert
            _repositoryMock.Verify();
            Assert.Equal($"The chat member with chat_id {chatId} user_id {userId} failed to update.", result.Message);
        }

        // InsertChatMemberAsync_Default_ReturnsChatMemberDto
        [Fact]
        public async Task InsertChatMemberAsync_Default_ReturnsChatMemberDto()
        {
            // arrange 
            int chatId = 1, userId = 1;
            User user = UserDtoFactory.CreateUser(userId: userId);
            UserDisplayDto userDto = UserDtoFactory.CreateUserDisplayDto(userId:userId, displayName:user.DisplayName);
            ChatMember member = ChatMemberDtoFactory.CreateChatMember(chatId: chatId, userId: userId);
            IEnumerable<ChatMember> chatMembers = [member];
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(userId: userId, chatId: chatId);
            _repositoryMock.Setup(r => r.User.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.GetChatMemberByChatIdAndUserIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(member).Verifiable();
            _repositoryMock.Setup(r => r.ChatMember.SetChatMemberStatus(It.IsAny<int>(), It.IsAny<int>(), 1)).ReturnsAsync(1).Verifiable();
            _cacheMock.Setup(c => c.RemoveDataAsync(It.IsAny<string>())).Verifiable(Times.Once);
            _mapperMock.Setup(m => m.Map<ChatMemberDto>(It.IsAny<ChatMember>())).Returns(chatMemberDto).Verifiable();
            _mapperMock.Setup(m => m.Map<UserDisplayDto>(It.IsAny<User>())).Returns(userDto).Verifiable();

            // act
            var result =await _serviceManager.ChatMemberService.InsertChatMemberAsync(chatId, userId);

            // assert
            _repositoryMock.Verify();
            _cacheMock.Verify();
            _mapperMock.Verify();
            Assert.IsType<ChatMemberDto>(result);
            Assert.Equivalent(chatMemberDto, result);
        }
    }
}
