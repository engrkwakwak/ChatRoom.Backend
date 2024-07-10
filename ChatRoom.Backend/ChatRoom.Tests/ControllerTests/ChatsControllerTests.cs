using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Controllers;
using ChatRoom.Backend.Presentation.Hubs;
using ChatRoom.UnitTest.Helpers;
using Contracts;
using Entities.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Moq;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Messages;
using Shared.DataTransferObjects.Users;
using Shared.Enums;
using Shared.RequestFeatures;

namespace ChatRoom.UnitTest.ControllerTests
{
    public class ChatsControllerTests
    {
        private readonly Mock<IServiceManager> _serviceMock = new();

        private readonly Mock<IHubContext<ChatRoomHub>> _hubContextMock = new();
        
        private readonly Mock<IFileManager> _fileManagerMock = new();

        private readonly ChatsController _controller;

        private readonly Mock<IHubClients> _clientsMock = new();

        private readonly Mock<IClientProxy> _clientProxyMock = new();


        public ChatsControllerTests()
        {
            _hubContextMock.Setup(hub => hub.Clients).Returns(_clientsMock.Object);
            _clientsMock.Setup(clients => clients.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
            _clientsMock.Setup(clients => clients.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);
            _clientsMock.Setup(clients => clients.Users(It.IsAny<IReadOnlyList<string>>())).Returns(_clientProxyMock.Object);

            //_hubContextMock.Object.Clients
            _controller = new ChatsController(_serviceMock.Object, _hubContextMock.Object, _fileManagerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        private void SetAuthorizationHeader()
        {
            KeyValuePair<string, StringValues> kp = new KeyValuePair<string, StringValues>("Authorization", "Bearertoken");
            _controller.Request.Headers.Add(kp);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetChatById_ChatIdLessThanOne_ReturnsInvalidParameterException(int chatId)
        {
            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.GetChatById(chatId));
            Assert.Equal("Something went wrong while processing the request. It looks like the chat is invalid.", result.Message);
        }

        [Fact]
        public async Task GetChatById_Succeeds_ReturnsOkObjectResult()
        {
            // Arrange
            ChatDto chatDto = ChatDtoFactory.CreateP2PChatDto();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatDto);

            // Act
            var result = await _controller.GetChatById(chatDto.ChatId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            OkObjectResult resultObject = (OkObjectResult)result;
            Assert.IsType<ChatDto>(resultObject.Value);
            Assert.Equal(chatDto.ChatId, ((ChatDto)resultObject.Value).ChatId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetChatMembers_ChatIdLessThanOne_ReturnInvalidParameterException(int chatId)
        {
            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.GetChatById(chatId));
            Assert.Equal("Something went wrong while processing the request. It looks like the chat is invalid.", result.Message);

        }

        [Fact]
        public async Task GetChatMembers_Succeeds_ReturnChatMembers()
        {
            // Arrange
            IEnumerable<ChatMemberDto> chatMemebers = ChatMemberDtoFactory.CreateChatMembersList();
            int chatId = chatMemebers.First().ChatId;
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMemebers);

            // Act
            var actual = await _controller.GetChatMembers(chatId);

            // Assert
            Assert.IsType<OkObjectResult>(actual);
            OkObjectResult actualResult = (OkObjectResult)actual;
            Assert.IsAssignableFrom<IEnumerable<ChatMemberDto>>(actualResult.Value);
        }

        [Fact]
        public void CreateChat_P2PChatModelIsNotValid_ReturnsUnprocessableEntity()
        {
            // Arrange
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateInValidP2PChatForCreationDto();
            var actionArguments = new Dictionary<string, object> { { "chat", chatForCreationDto } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(ChatsController.CreateChat),
                nameof(ChatsController),
                actionArguments,
                validationFilter
            );

            // Act
            ModelValidator.ValidateModel(chatForCreationDto, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);
        }

        [Fact]
        public void CreateChat_GroupChatModelIsNotValid_ReturnsUnprocessableEntity()
        {
            // Arrange
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateInValidGroupChatForCreationDto();
            var actionArguments = new Dictionary<string, object> { { "chat", chatForCreationDto } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(ChatsController.CreateChat),
                nameof(ChatsController),
                actionArguments,
                validationFilter
            );

            // Act
            ModelValidator.ValidateModel(chatForCreationDto, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);
        }

        [Fact]
        public async Task CreateChat_IsP2PChatAndIsAlreadyExist_ReturnsExistingChat()
        {
            // Arrange
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateValidP2PChatForCreationDto();
            var actionArguments = new Dictionary<string, object> { { "chat", chatForCreationDto } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(ChatsController.CreateChat),
                nameof(ChatsController),
                actionArguments,
                validationFilter
            );
            SetAuthorizationHeader();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(chatForCreationDto.ChatMemberIds!.First());
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(UserDtoFactory.CreateUserDto());
            _serviceMock.Setup(s => s.ChatService.GetP2PChatByUserIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(ChatDtoFactory.CreateP2PChatDto());

            // Act
            ModelValidator.ValidateModel(chatForCreationDto, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;
            var result = await _controller.CreateChat(chatForCreationDto);

            // Assert
            _serviceMock.VerifyAll();
            Assert.Null(filterResult);
            Assert.IsType<OkObjectResult>(result);
            OkObjectResult objectResult = (OkObjectResult)result;   
            Assert.NotNull(objectResult);
            Assert.IsType<ChatDto>(objectResult.Value);
        }

        [Fact]
        public async Task CreateChat_IsP2PChatNewAndCreationSuccess_ReturnsCreatedAtRoute()
        {
            // Arrange
            ChatDto? chat = null;
            ChatDto? _chat = ChatDtoFactory.CreateP2PChatDto();
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateValidP2PChatForCreationDto();
            SetAuthorizationHeader();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(chatForCreationDto.ChatMemberIds!.First()).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetP2PChatByUserIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(chat!).Verifiable();
            _serviceMock.Setup(s => s.ChatService.CreateChatWithMembersAsync(It.IsAny<ChatForCreationDto>())).ReturnsAsync(ChatDtoFactory.CreateP2PChatDto()).Verifiable();

            // Act
            var result = await _controller.CreateChat(chatForCreationDto);

            // Assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Users(It.IsAny<IReadOnlyList<string>>()));
            Assert.IsType<CreatedAtRouteResult>(result);
            CreatedAtRouteResult objectResult = (CreatedAtRouteResult)result;
            Assert.NotNull(objectResult);
            Assert.IsType<ChatDto>(objectResult.Value);
            Assert.Equal("GetChatByChatId", objectResult.RouteName);
            Assert.True(objectResult.RouteValues?.ContainsKey("chatId"));
        }

        [Fact]
        public async Task CreateChat_IsGCAndSetIsAdminAsyncIsFalse_ThrowsUserUpdateFailedException()
        {
            // Arrange
            SetAuthorizationHeader();
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateValidGroupChatForCreationDto();
            int adminId = chatForCreationDto.ChatMemberIds!.First();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(adminId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.ChatService.CreateChatWithMembersAsync(It.IsAny<ChatForCreationDto>())).ReturnsAsync(ChatDtoFactory.CreateGroupChatDto()).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetIsAdminAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(false).Verifiable();

            // Act
            var result = await Assert.ThrowsAsync<UserUpdateFailedException>(async () => await _controller.CreateChat(chatForCreationDto));

            // Assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Users(It.IsAny<IReadOnlyList<string>>()));
            Assert.NotNull(result);
            Assert.Equal($"The server failed to update the user with id: {adminId}.", result.Message);
        }

        [Fact]
        public async Task CreateChat_IsGCAndSetIsAdminAsyncIsTrue_ReturnsCreatedAtRoute()
        {
            // Arrange
            SetAuthorizationHeader();
            MessageDto messageDto = new();
            ChatForCreationDto chatForCreationDto = ChatDtoFactory.CreateValidGroupChatForCreationDto();
            int adminId = chatForCreationDto.ChatMemberIds!.First();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(adminId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(UserDtoFactory.CreateUserDto()).Verifiable();
            _serviceMock.Setup(s => s.ChatService.CreateChatWithMembersAsync(It.IsAny<ChatForCreationDto>())).ReturnsAsync(ChatDtoFactory.CreateGroupChatDto()).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetIsAdminAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(true).Verifiable();
            _serviceMock.Setup(s => s.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>())).ReturnsAsync(messageDto).Verifiable();

            // Act
            var result = await _controller.CreateChat(chatForCreationDto);

            // Assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Users(It.IsAny<IReadOnlyList<string>>()), Times.Exactly(2));
            Assert.IsType<CreatedAtRouteResult>(result);
            CreatedAtRouteResult objectResult = (CreatedAtRouteResult)result;
            Assert.NotNull(objectResult);
            Assert.IsType<ChatDto>(objectResult.Value);
            Assert.Equal("GetChatByChatId", objectResult.RouteName);
            Assert.True(objectResult.RouteValues?.ContainsKey("chatId"));
        }

        [Fact]
        public async Task UpdateUserLastSeenMessage_Succeeds_ReturnsNoContentResult()
        {
            // Arrange
            int chatId = 1;
            int userId = 1;
            ChatMemberForUpdateDto chatMemberForUpdateDto = new();
            _serviceMock.Setup(
                s => s.ChatMemberService.UpdateLastSeenMessageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ChatMemberForUpdateDto>())
                ).ReturnsAsync(ChatMemberDtoFactory.CreateChatMemberDto());

            // Act
            var result = await _controller.UpdateUserLastSeenMessage(chatId, userId, chatMemberForUpdateDto);

            // Assert
            _serviceMock.VerifyAll();
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()));
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_IsGroupChatAndUserIsNotAdmin_ThrowsUnauthorizedChatDeletion()
        {
            // Arrange
            SetAuthorizationHeader();
            int chatId = 1, userId = 1;
            ChatDto chat = ChatDtoFactory.CreateGroupChatDto();
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto();
            _serviceMock.Setup(s => s.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId)).ReturnsAsync(chatMemberDto).Verifiable();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId)).ReturnsAsync(ChatMemberDtoFactory.CreateChatMembersList()).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(chatId)).ReturnsAsync(chat).Verifiable();

            // Act
            var result = await Assert.ThrowsAsync<UnauthorizedChatDeletion>(async () => await _controller.Delete(chatId));

            // Assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal("Unauthorized Action detected. Access for this action is for chat admins only.", result.Message);
        }

        [Fact]
        public async Task Delete_DeleteChatAsyncFailed_ThrowsChatNotDeletedException()
        {
            // Arrange
            SetAuthorizationHeader();
            int chatId = 1, userId = 1;
            ChatDto chat = ChatDtoFactory.CreateGroupChatDto();
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(true);
            _serviceMock.Setup(s => s.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId)).ReturnsAsync(chatMemberDto).Verifiable();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId)).ReturnsAsync(ChatMemberDtoFactory.CreateChatMembersList()).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(chatId)).ReturnsAsync(chat).Verifiable();
            _serviceMock.Setup(s => s.ChatService.DeleteChatAsync(chatId)).ReturnsAsync(false).Verifiable();

            // Act
            var result = await Assert.ThrowsAsync<ChatNotDeletedException>(async () => await _controller.Delete(chatId));

            // Assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal("Something went wrong while deleting the chat. Please try again later", result.Message);
        }

        [Fact]
        public async Task Delete_HasDisplayPicture_ReturnsNoContentResult()
        {
            // Arrange
            SetAuthorizationHeader();
            int chatId = 1, userId = 1;
            ChatDto chat = ChatDtoFactory.CreateGroupChatDto("dislay-picture-url");
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(true);
            _serviceMock.Setup(s => s.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId)).ReturnsAsync(chatMemberDto).Verifiable();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId)).ReturnsAsync(ChatMemberDtoFactory.CreateChatMembersList()).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(chatId)).ReturnsAsync(chat).Verifiable();
            _serviceMock.Setup(s => s.ChatService.DeleteChatAsync(chatId)).ReturnsAsync(true).Verifiable();
            _fileManagerMock.Setup(fm => fm.DeleteImageAsync(It.IsAny<string>())).Verifiable();

            // Act
            var result = await _controller.Delete(chatId);

            // Assert
            _serviceMock.Verify();
            _fileManagerMock.Verify(fm => fm.DeleteImageAsync(It.IsAny<string>()), Times.Once());
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_HasNoDisplayPicture_ReturnsNoContentResult()
        {
            // Arrange
            SetAuthorizationHeader();
            int chatId = 1, userId = 1;
            ChatDto chat = ChatDtoFactory.CreateGroupChatDto();
            ChatMemberDto chatMemberDto = ChatMemberDtoFactory.CreateChatMemberDto(true);
            _serviceMock.Setup(s => s.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId)).ReturnsAsync(chatMemberDto).Verifiable();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId)).ReturnsAsync(ChatMemberDtoFactory.CreateChatMembersList()).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(chatId)).ReturnsAsync(chat).Verifiable();
            _serviceMock.Setup(s => s.ChatService.DeleteChatAsync(chatId)).ReturnsAsync(true).Verifiable();
            _fileManagerMock.Setup(fm => fm.DeleteImageAsync(It.IsAny<string>())).Verifiable();

            // Act
            var result = await _controller.Delete(chatId);

            // Assert
            _serviceMock.Verify();
            _fileManagerMock.Verify(fm => fm.DeleteImageAsync(It.IsAny<string>()), Times.Never());
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CanViewChat_Success_ReturnsBoolean()
        {
            // Arrange
            SetAuthorizationHeader();
            int chatId = 1, userId = 1;
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.ChatService.CanViewAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _controller.CanViewChat(chatId);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True(result);
        }

        [Fact]
        public async Task GetChatListByUserId_Success_ReturnsOkObjectResult()
        {
            // Arrange
            ChatParameters parameters = new()
            {
                UserId = "1",
                PageSize = 10,
                PageNumber = 1,
                Name = "Test",
            };
            _serviceMock.Setup(s => s.ChatService.GetChatListByChatIdAsync(It.IsAny<ChatParameters>())).ReturnsAsync(ChatDtoFactory.CreateChatDtos()).Verifiable();

            // Act
            var result = await _controller.GetChatListByUserId(parameters);

            // Assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.IsAssignableFrom<IEnumerable<ChatDto>>(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task TypingStart_Success_ReturnsNoContentResult()
        {
            // Arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1;
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetChatMemberByChatIdUserIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(ChatMemberDtoFactory.CreateChatMemberDto()).Verifiable();

            // Act
            var result = await _controller.TypingStart(chatId);

            // Assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()));
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task TypingEnd_Success_ReturnsNoContentResult()
        {
            // Arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1;
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetChatMemberByChatIdUserIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(ChatMemberDtoFactory.CreateChatMemberDto()).Verifiable();

            // Act
            var result = await _controller.TypingEnd(chatId);

            // Assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()));
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Leave_MemberIsTheOnlyAdmin_ThrowsInvalidParameterException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.Leave(chatId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal("Invalid request. You cannot leave the chat because you are the only admin left. Please assgin another admin before leaving the chat.", result.Message);

        }

        [Fact]
        public async Task Leave_MemberFailedToLeave_ThrowsChatMemberNotUpdatedException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersListWithMultipleAdmin();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetChatMemberStatus(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<ChatMemberNotUpdatedException>(async () => await _controller.Leave(chatId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal($"The chat member with chat_id {chatId} user_id {userId} failed to update.", result.Message);
        }

        [Fact]
        public async Task Leave_MemberSuccessfullyLeave_ReturnsNoContentResult()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto(userId: userId);
            MessageDto messageDto = new();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersListWithMultipleAdmin();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetChatMemberStatus(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true).Verifiable();
            _serviceMock.Setup(s => s.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>())).ReturnsAsync(messageDto).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(It.IsAny<int>())).ReturnsAsync(ChatDtoFactory.CreateGroupChatDto()).Verifiable();

            // act
            var result = await _controller.Leave(chatId);

            // assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Exactly(2));
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task AddMember_ChatMemberIsNotAdmin_ThrowsUnauthorizedChatActionException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 4, chatId = 1, memmberUserId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersListWithMultipleAdmin();
            chatMembers = chatMembers.Append(new()
            {
                ChatId = 1,
                StatusId = 1,
                IsAdmin = false,
                UserId = userId,
                User = new()
                {
                    DisplayName = "Test Name 2",
                    UserId = userId
                },
            });
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();

            // Act
            var result = await Assert.ThrowsAsync<UnauthorizedChatActionException>(async () => await _controller.AddMember(chatId, memmberUserId));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Unauthorized Action detected. Access for this action is for chat admins only.", result.Message);
        }

        [Fact]
        public async Task AddMember_AddMemberStatusSuccess_ReturnsOkObjectResult()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 4, chatId = 1, memberUserId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersListWithMultipleAdmin();
            chatMembers = chatMembers.Append(new()
            {
                ChatId = 1,
                StatusId = 1,
                IsAdmin = true,
                UserId = userId,
                User = new()
                {
                    DisplayName = "Test Name 2",
                    UserId = userId
                }
            });
            MessageDto messageDto = new();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.InsertChatMemberAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(ChatMemberDtoFactory.CreateChatMemberDto()).Verifiable();
            _serviceMock.Setup(s => s.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>())).ReturnsAsync(messageDto).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(It.IsAny<int>())).ReturnsAsync(ChatDtoFactory.CreateGroupChatDto()).Verifiable();

            // act
            var result = await _controller.AddMember(chatId, memberUserId);

            // assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Exactly(2));
            _hubContextMock.Verify(h => h.Clients.User(It.IsAny<string>()), Times.Exactly(1));
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ChatMemberDto>(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task SetAdmin_ChatMemberIsNotAdmin_ThrowsUnauthorizedChatActionException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 2, chatId = 1, memberUserId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<UnauthorizedChatActionException>(async () => await _controller.SetAdmin(chatId, memberUserId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal("Unauthorized Action detected. Access for this action is for chat admins only.", result.Message);
        }

        [Fact]
        public async Task SetAdmin_SettingOfAdminFailed_ThrowsChatMemberNotUpdatedException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1, memberUserId = 2;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetIsAdminAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(false).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<ChatMemberNotUpdatedException>(async () => await _controller.SetAdmin(chatId, memberUserId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal($"The chat member with chat_id {chatId} user_id {memberUserId} failed to update.", result.Message);
        }

        [Fact]
        public async Task SetAdmin_SettingOfAdminSucceeds_ReturnsNoContentResult()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1, memberUserId = 2;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetIsAdminAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(true).Verifiable();
            MessageDto messageDto = new();
            _serviceMock.Setup(s => s.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>())).ReturnsAsync(messageDto).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(It.IsAny<int>())).ReturnsAsync(ChatDtoFactory.CreateGroupChatDto()).Verifiable();

            // act
            var result = await _controller.SetAdmin(chatId, memberUserId);

            // assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Exactly(2));
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveAdmin_ChatMemberIsNotAdmin_ThrowsUnauthorizedChatActionException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 2, chatId = 1, memberUserId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<UnauthorizedChatActionException>(async () => await _controller.RemoveAdmin(chatId, memberUserId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal("Unauthorized Action detected. Access for this action is for chat admins only.", result.Message);
        }

        [Fact]
        public async Task RemoveAdmin_RemovingAdminFailed_ThrowsChatMemberNotUpdatedException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1, memberUserId = 2;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetIsAdminAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(false).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<ChatMemberNotUpdatedException>(async () => await _controller.RemoveAdmin(chatId, memberUserId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal($"The chat member with chat_id {chatId} user_id {memberUserId} failed to update.", result.Message);
        }

        [Fact]
        public async Task RemoveAdmin_RemovingAdminSucceeds_ReturnsNoContentResult()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1, memberUserId = 2;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetIsAdminAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(true).Verifiable();
            MessageDto messageDto = new();
            _serviceMock.Setup(s => s.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>())).ReturnsAsync(messageDto).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(It.IsAny<int>())).ReturnsAsync(ChatDtoFactory.CreateGroupChatDto()).Verifiable();

            // act
            var result = await _controller.RemoveAdmin(chatId, memberUserId);

            // assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Exactly(2));
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveMember_MemberIsNotAdmin_ThrowsUnauthorizedChatActionException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 2, chatId = 1, memberUserId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();

            // act 
            var result = await Assert.ThrowsAsync<UnauthorizedChatActionException>(async () => await _controller.RemoveMember(chatId, memberUserId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal("Unauthorized Action detected. Access for this action is for chat admins only.", result.Message);
        }

        [Fact]
        public async Task RemoveMember_MemberRemovingItself_ThrowsUnauthorizedChatActionException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1, memberUserId = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();

            // act 
            var result = await Assert.ThrowsAsync<UnauthorizedChatActionException>(async () => await _controller.RemoveMember(chatId, memberUserId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal("Unauthorized Action detected. You cannot remove yourself from the chat", result.Message);
        }

        [Fact]
        public async Task RemoveMember_RemovingMemberFailed_ThrowsChatMemberNotUpdatedException()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1, memberUserId = 2;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetChatMemberStatus(It.IsAny<int>(), It.IsAny<int>(), (int)Status.Deleted)).ReturnsAsync(false).Verifiable();

            // act 
            var result = await Assert.ThrowsAsync<ChatMemberNotUpdatedException>(async () => await _controller.RemoveMember(chatId, memberUserId));

            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.Equal($"The chat member with chat_id {chatId} user_id {memberUserId} failed to update.", result.Message);
        }

        [Fact]
        public async Task RemoveMember_RemovingMemberSucceeds_ReturnsNoContentResult()
        {
            // arrange
            SetAuthorizationHeader();
            int userId = 1, chatId = 1, memberUserId = 2;
            UserDto userDto = UserDtoFactory.CreateUserDto(userId), removedUser = UserDtoFactory.CreateUserDto(memberUserId);
            ChatDto chatDto = ChatDtoFactory.CreateGroupChatDto(chatId: chatId);
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.SetChatMemberStatus(It.IsAny<int>(), It.IsAny<int>(), (int)Status.Deleted)).ReturnsAsync(true).Verifiable();
            MessageDto messageDto = new();
            _serviceMock.Setup(s => s.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>())).ReturnsAsync(messageDto).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(removedUser).Verifiable();
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(chatId)).ReturnsAsync(chatDto).Verifiable();

            // act 
            var result = await _controller.RemoveMember(chatId, memberUserId);

            // assert
            _serviceMock.Verify();
            _serviceMock.Verify(s => s.ChatService.GetChatByChatIdAsync(chatId), Times.Exactly(2));
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Exactly(2));
            _hubContextMock.Verify(h => h.Clients.User(It.IsAny<string>()), Times.Exactly(1));
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        public async Task Member_ChatIdOrMemberUserIdIsLessThanOne_ReturnOkObjectResult(int chatId, int memberUserId)
        {
            // act
            var result = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.Member(chatId, memberUserId));

            // assert
            Assert.NotNull(result);
            Assert.Equal("Something went wrong while fetching the member. The chat id or member id is invalid.", result.Message);
        }

        [Fact]
        public async Task Member_Succeeds_ReturnOkObjectResult()
        {
            // arrange
            int chatId = 1, memberUserId = 1;
            _serviceMock.Setup(s => s.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, memberUserId)).ReturnsAsync(ChatMemberDtoFactory.CreateChatMemberDto()).Verifiable();

            // act
            var result = await _controller.Member(chatId, memberUserId);
            
            // assert
            _serviceMock.Verify();
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ChatMemberDto>(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task UpdateChat_Succeeds_ReturnsNoContentResult()
        {
            // arrange
            int userId = 1, chatId = 1;
            ChatForUpdateDto chatForUpdateDto = ChatDtoFactory.CreateChatForUpdateDto();
            SetAuthorizationHeader();
            UserDto userDto = UserDtoFactory.CreateUserDto();
            IEnumerable<ChatMemberDto> chatMembers = ChatMemberDtoFactory.CreateChatMembersList();
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(It.IsAny<string>())).Returns(userId).Verifiable();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto).Verifiable();
            _serviceMock.Setup(s => s.ChatMemberService.GetActiveChatMembersByChatIdAsync(It.IsAny<int>())).ReturnsAsync(chatMembers).Verifiable();
            MessageDto messageDto = new();
            _serviceMock.Setup(s => s.MessageService.InsertMessageAsync(It.IsAny<MessageForCreationDto>())).ReturnsAsync(messageDto).Verifiable();;
            _serviceMock.Setup(s => s.ChatService.GetChatByChatIdAsync(chatId)).ReturnsAsync(ChatDtoFactory.CreateGroupChatDto()).Verifiable();

            // act
            var result = await _controller.UpdateChat(chatId, chatForUpdateDto);

            // assert
            _serviceMock.Verify();
            _hubContextMock.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Exactly(3));
            _hubContextMock.Verify();
            Assert.IsType<NoContentResult>(result);
        }
    }
}
