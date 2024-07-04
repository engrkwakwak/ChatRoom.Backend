using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Controllers;
using ChatRoom.UnitTest.Helpers;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NuGet.Frameworks;
using Service.Contracts;
using Shared.DataTransferObjects.Users;

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

            UserDto user = DtoFactory.CreateUserDto();

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
            UserForUpdateDto user = DtoFactory.CreateUserForUpdateDto();
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
        public void UpdateUser_InvalidModelState_ReturnsUnprocessableEntity() {
            // Arrange
            UserForUpdateDto user = DtoFactory.CreateInvalidUserForUpdateDto();
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
            var str = filterResult.Value;
        }

        [Fact]
        public async Task UpdateUser_ValidParameters_ReturnsNoContent() {

        }
    }
}
