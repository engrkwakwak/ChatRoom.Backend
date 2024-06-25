using ChatRoom.Backend.Presentation.Controllers;
using Entities.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Org.BouncyCastle.Crypto;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
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
        public async Task SignUp_SendVerificationEmailIsFalse_ReturnsBadRequest()
        {
            // Cannot mock HttpContext needs more research

            //SignUpDto user = new()
            //{
            //    Email = "test@email.com",
            //    Username = "TestUser",
            //    Password = "password",
            //    PasswordConfirmation = "password",
            //    DisplayName = "Test Name"
            //};
            //UserDto createdUser = new()
            //{
            //    Username = user.Username,
            //    DisplayName = user.DisplayName!,
            //    Email = user.Email,
            //    UserId = 1
            //};
            //_serviceMock.Setup(s => s.UserService.HasDuplicateEmailAsync(user.Email!)).ReturnsAsync(false);
            //_serviceMock.Setup(s => s.UserService.HasDuplicateUsernameAsync(user.Username!)).ReturnsAsync(false);
            //_serviceMock.Setup(s => s.UserService.InsertUserAsync(user)).ReturnsAsync(createdUser);
            //_serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(createdUser)).Returns("test-token");
            //_serviceMock.Setup(s => s.EmailService.SendVerificationEmail(createdUser, "")).ReturnsAsync(false);

            //_controller.HttpContext.Request.Host = HostString.FromUriComponent("localhost:5100");

            //var actual = await Assert.ThrowsAsync<EmailNotSentException>(async () =>
            //{
            //    await _controller.SignUp(user);
            //});
            //_serviceMock.VerifyAll();
            //Assert.Equal($"Something went wrong while sending the verification email.", actual.Message);
            Assert.Fail();
        }

        [Fact]
        public async Task UpdatePassword_PasswordsDoesntMatch_ReturnsValidationException()
        {
            Assert.Fail();
        }
    }
}
