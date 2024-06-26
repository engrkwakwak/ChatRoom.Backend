using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Controllers;
using ChatRoom.UnitTest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(_controller, actionArguments, validationFilter);

            // Act
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var result = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetUserById_ValidUserId_ReturnsStatus200Ok() {
            // Arrange
            int userId = 1;
            var actionArguments = new Dictionary<string, object> { { "userId", userId } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(_controller, actionArguments, validationFilter);

            UserDto user = new() {
                DisplayName = "Test",
                Email = "Test",
                Username = "Test",
            };

            _serviceMock.Setup(x => x.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var result = await _controller.GetUserById(userId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task HasDuplicateEmail_EmailAlreadyExists_ReturnsStatus200Ok() {
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
    }
}
