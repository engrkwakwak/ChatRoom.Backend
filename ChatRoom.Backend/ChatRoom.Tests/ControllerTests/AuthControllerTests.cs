using ChatRoom.Backend.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using System.ComponentModel.DataAnnotations;

namespace ChatRoom.UnitTest.ControllerTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IServiceManager> _serviceMock = new();

        private readonly Mock<IConfiguration> _configurationMock =  new();

        private readonly AuthController _controller;

        public AuthControllerTests() 
        { 
            _controller = new AuthController(_serviceMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Authenticate_InvalidInputs_ReturnsValidationError()
        {
            Assert.Fail();
        }

        [Fact]
        public async Task Authenticate_ValidateUserIsFalse_ReturnsUnauthorizedStatus()
        {
            SignInDto user = new();
            _serviceMock.Setup(s => s.AuthService.ValidateUser(user)).ReturnsAsync(false);

            IActionResult actual = await _controller.Authenticate(user);

            _serviceMock.Verify(m => m.AuthService.ValidateUser(user), Times.Once);
            Assert.IsType<UnauthorizedResult>(actual);
        }

        [Fact]
        public async Task Authenticate_ValidateUserIsTrue_ReturnsOkStatus()
        {
            SignInDto user = new();
            _serviceMock.Setup(s => s.AuthService.ValidateUser(user)).ReturnsAsync(true);

            IActionResult actual = await _controller.Authenticate(user);

            _serviceMock.Verify(m => m.AuthService.ValidateUser(user), Times.Once);
            Assert.IsType<OkObjectResult>(actual);
        }

        [Fact]  
        public async Task SignUp_HasDuplicateEmail_ReturnsValidationException()
        {
            SignUpDto user = new()
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "password",
                PasswordConfirmation = "password"
            };
            _serviceMock.Setup(s => s.UserService.HasDuplicateEmailAsync(user.Email!)).ReturnsAsync(true);

            var actual = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _controller.SignUp(user);
                _serviceMock.Verify(m => m.UserService.HasDuplicateEmailAsync(user.Email!), Times.Once);
            });
            Assert.Equal($"The email {user.Email} is already in used by another user.", actual.Message);
        }

        [Fact]  
        public async Task SignUp_HasDuplicateUsername_ReturnsValidationException()
        {
            SignUpDto user = new()
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "password",
                PasswordConfirmation = "password"
            };
            _serviceMock.Setup(s => s.UserService.HasDuplicateEmailAsync(user.Email!)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.UserService.HasDuplicateUsernameAsync(user.Username!)).ReturnsAsync(true);

            var actual = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _controller.SignUp(user);
                _serviceMock.VerifyAll();
            });
            
            Assert.Equal($"The username {user.Username} is already in used by another user.", actual.Message);
        }

        [Fact]  
        public async Task SignUp_PasswordsDontMatch_ReturnsValidationException()
        {
            SignUpDto user = new()
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "password",
                PasswordConfirmation = "password1"
            };
            _serviceMock.Setup(s => s.UserService.HasDuplicateEmailAsync(user.Email!)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.UserService.HasDuplicateUsernameAsync(user.Username!)).ReturnsAsync(false);

            var actual = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _controller.SignUp(user);
                _serviceMock.VerifyAll();
            });
            Assert.Equal($"The Password and Password Confirmation didnt match.", actual.Message);
        }

        [Fact]
        public async Task SignUp_SendVerificationEmailIsfalse_ReturnsBadRequest()
        {
            Assert.Fail();
        }
    }
}
