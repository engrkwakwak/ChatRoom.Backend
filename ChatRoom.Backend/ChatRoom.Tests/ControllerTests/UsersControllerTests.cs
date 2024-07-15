using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Controllers;
using ChatRoom.UnitTest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Contracts;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;
using System.Text.Json;

namespace ChatRoom.UnitTest.ControllerTests {
    public class UsersControllerTests {
        private readonly Mock<IServiceManager> _serviceMock;
        private readonly UsersController _controller;

        public UsersControllerTests() {
            _serviceMock = new Mock<IServiceManager>();
            _controller = new UsersController(_serviceMock.Object) {
                ControllerContext = new ControllerContext {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetUserById_InvalidUserId_ReturnsBadRequest(int userId) {
            // Arrange
            var actionArguments = new Dictionary<string, object> { { "userId", userId } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(_controller, nameof(UsersController.GetUserById), nameof(UsersController), actionArguments, validationFilter);
            string expectedMessage = $"userId must be greater than zero. Controller: {nameof(UsersController)}, action: {nameof(UsersController.GetUserById)}";

            // Act
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var result = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(result.Value, expectedMessage);
        }

        [Fact]
        public async Task GetUserById_ValidUserId_ReturnsStatus200Ok() {
            // Arrange
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(_controller, nameof(UsersController.GetUserById), nameof(UsersController), actionArguments, validationFilter);

            UserDto user = UserDtoFactory.CreateUserDto();

            _serviceMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;
            var result = await _controller.GetUserById(userId) as ObjectResult;

            // Assert
            Assert.Null(filterResult);
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task HasDuplicateEmail_EmailAlreadyExists_ReturnsTrue() {
            // Arrange
            string email = "test@test.com";
            _serviceMock.Setup(x => x.UserService.HasDuplicateEmailAsync(email)).ReturnsAsync(true);

            // Act
            var result = await _controller.HasDuplicateEmail(email) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);
            Assert.True((bool)result.Value!);
        }

        [Fact]
        public async Task HasDuplicateEmail_EmailDoesNotExist_ReturnsFalse() {
            // Arrange
            string email = "test@test.com";
            _serviceMock.Setup(x => x.UserService.HasDuplicateEmailAsync(email)).ReturnsAsync(false);

            // Act
            var result = await _controller.HasDuplicateEmail(email) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);
            Assert.False((bool)result.Value!);
        }

        [Fact]
        public async Task HasDuplicateUsername_UsernameAlreadyExists_ReturnsTrue() {
            // Arrange
            string username = "testusername";
            _serviceMock.Setup(x => x.UserService.HasDuplicateUsernameAsync(username)).ReturnsAsync(true);

            // Act
            var result = await _controller.HasDuplicateUsername(username) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);
            Assert.True((bool)result.Value!);
        }

        [Fact]
        public async Task HasDuplicateUsername_UsernameDoesNotExist_ReturnsFalse() {
            // Arrange
            string username = "testusername";
            _serviceMock.Setup(x => x.UserService.HasDuplicateUsernameAsync(username)).ReturnsAsync(false);

            // Act
            var result = await _controller.HasDuplicateUsername(username) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);
            Assert.False((bool)result.Value!);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UpdateUser_InvalidUserId_ReturnsBadRequest(int userId) {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDto();
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(_controller, nameof(UsersController.UpdateUser), nameof(UsersController), actionArguments, validationFilter);
            string expectedMessage = $"userId must be greater than zero. Controller: {nameof(UsersController)}, action: {nameof(UsersController.UpdateUser)}";

            // Act
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var result = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(result.Value, expectedMessage);
        }

        [Fact]
        public void UpdateUser_EmptyUsername_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithEmptyUsername();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("Username", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("Username is required.", errorMessageList);
        }

        [Fact]
        public void UpdateUser_UsernameExceeds20Characters_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithUsernameGreaterThan20Characters();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("Username", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("The maximum length for 'username' is 20 characters.", errorMessageList);
        }

        [Fact]
        public void UpdateUser_EmptyDisplayName_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithEmptyDisplayName();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("DisplayName", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("Display name is required.", errorMessageList);
        }

        [Fact]
        public void UpdateUser_DisplayNameExceeds50Characters_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithDisplayNameGreaterThan50Characters();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("DisplayName", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("The maximum length for 'display name' is 50 characters.", errorMessageList);
        }

        [Fact]
        public void UpdateUser_EmptyEmail_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithEmptyEmail();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("Email", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("Email is required.", errorMessageList);
        }

        [Fact]
        public void UpdateUser_EmailExceeds100Characters_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithEmailGreaterThan100Characters();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("Email", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("The maximum length for 'email' is 100 characters.", errorMessageList);
        }

        [Fact]
        public void UpdateUser_InvalidEmailFormat_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithInvalidEmailFormat();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("Email", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("The Email field is not a valid e-mail address.", errorMessageList);
        }

        [Fact]
        public void UpdateUser_AddressExceeds100Characters_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithAddressGreaterThan100Characters();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("Address", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("The maximum length for 'address' is 100 characters.", errorMessageList);
        }

        [Fact]
        public void UpdateUser_DisplayPictureUrlExceeds200Characters_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDtoWithDisplayPictureUrlGreaterThan200Characters();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

            string[] errorMessageList = [];
            if (filterResult.Value is SerializableError errResponse && errResponse.TryGetValue("DisplayPictureUrl", out var errorMessages)) {
                errorMessageList = Assert.IsType<string[]>(errorMessages);
            }
            Assert.Contains("The maximum length for 'picture path' is 200 characters.", errorMessageList);
        }

        [Fact]
        public async Task UpdateUser_ValidParameters_ReturnsNoContent() {
            // Arrange
            UserForUpdateDto user = UserDtoFactory.CreateUserForUpdateDto();
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId }, { "userForUpdate", user } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(UsersController.UpdateUser), nameof(UsersController),
                actionArguments, validationFilter);

            _serviceMock.Setup(x => x.UserService.UpdateUserAsync(userId, user));

            // Act
            ModelValidator.ValidateModel(user, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;
            var result = await _controller.UpdateUser(userId, user) as NoContentResult;

            // Assert
            Assert.Null(filterResult);
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, result.StatusCode);

            _serviceMock.Verify(x => x.UserService.UpdateUserAsync(userId, user), Times.Once());
        }

        [Fact]
        public async Task GetAllUsers_NotSpecifiedParameters_UsesTheParameterDefaultValues() {
            // Arrange
            UserParameters parameters = new();
            MetaData metaData = new();
            IEnumerable<UserDto> users = UserDtoFactory.CreateListOfUserDto();
            _serviceMock.Setup(x => x.UserService.GetUsersAsync(parameters)).ReturnsAsync((users, metaData));

            // Act
            var result = await _controller.GetAllUsers(parameters) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);

            _serviceMock.Verify(x => x.UserService.GetUsersAsync(It.Is<UserParameters>(p =>
               p.PageNumber == 1 && p.PageSize == 10)), Times.Once);
        }

        [Fact]
        public async Task GetAllUsers_EmptyUsersList_ReturnsOkWithEmptyListAndPaginationHeader() {
            // Arrange
            UserParameters parameters = new();
            MetaData metaData = new();
            IEnumerable<UserDto> users = UserDtoFactory.CreateEmptyListOfUserDto();
            _serviceMock.Setup(x => x.UserService.GetUsersAsync(parameters)).ReturnsAsync((users, metaData));

            // Act
            var result = await _controller.GetAllUsers(parameters) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);

            var actualUsers = result.Value as IEnumerable<UserDto>;
            Assert.NotNull(actualUsers);
            Assert.Equal(users, actualUsers);

            var paginationHeader = _controller.Response.Headers["X-Pagination"].ToString();
            Assert.NotEmpty(paginationHeader);
            Assert.Equal(JsonSerializer.Serialize(metaData), paginationHeader);

            _serviceMock.Verify(x => x.UserService.GetUsersAsync(It.Is<UserParameters>(p =>
               p.PageNumber == 1 && p.PageSize == 10)), Times.Once);
        }
    }
}
