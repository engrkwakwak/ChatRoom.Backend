using ChatRoom.Backend.Presentation.Controllers;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using static Org.BouncyCastle.Math.EC.ECCurve;

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
        public void Authenticate_InvalidInputs_ReturnsValidationError()
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
        public void SignUp_SendVerificationEmailIsFalse_ReturnsBadRequest()
        {
            /*
            ** Cannot mock HttpContext needs more research
            */

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
            UpdatePasswordDto updatePasswordDto = new()
            {
                Password = "password",
                PasswordConfirmation = "password_",
                Token = ""
            };

            var actual = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _controller.UpdatePassword(updatePasswordDto);
            });
            Assert.Equal("The passwords doesnt match.", actual.Message);
        }

        [Fact]
        public async Task UpdatePassword_UpdatePasswordFailed_ReturnsUserUpdateFailedException()
        {
            int userId = 1;
            UpdatePasswordDto updatePasswordDto = new()
            {
                Password = "password",
                PasswordConfirmation = "password",
                Token = It.IsAny<string>(),
            };
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(It.IsAny<string>())).ReturnsAsync(false);
            _serviceMock.Setup(s => s.AuthService.VerifyJwtToken(updatePasswordDto.Token)).Returns(It.IsAny<JwtPayload>());
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(updatePasswordDto.Token)).Returns(userId);
            _serviceMock.Setup(s => s.UserService.UpdatePasswordAsync(userId, updatePasswordDto.Password)).ReturnsAsync(false);

            var actual = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await _controller.UpdatePassword(updatePasswordDto);
            });

            _serviceMock.VerifyAll();
            Assert.IsType<UserUpdateFailedException>(actual);
        }
          

        [Fact]
        public async Task UpdatePassword_EmailTokenIsUsed_ReturnsInvalidParameterException()
        {
            UpdatePasswordDto updatePasswordDto = new()
            {
                Password = "password",
                PasswordConfirmation = "password",
                Token = It.IsAny<string>(),
            };
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(It.IsAny<string>())).ReturnsAsync(true);

            var actual = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await _controller.UpdatePassword(updatePasswordDto);
            });

            _serviceMock.VerifyAll();
            Assert.IsType<InvalidParameterException>(actual);
            Assert.Equal($"Invalid Link. This link has expired or already been used.", actual.Message);
        }

        [Fact]
        public async Task UpdatePassword_UpdatePasswordSucceeds_ReturnsNoContent()
        {
            int userId = 1;
            UpdatePasswordDto updatePasswordDto = new()
            {
                Password = "password",
                PasswordConfirmation = "password",
                Token = It.IsAny<string>(),
            };
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(It.IsAny<string>())).ReturnsAsync(false);
            _serviceMock.Setup(s => s.AuthService.VerifyJwtToken(updatePasswordDto.Token)).Returns(It.IsAny<JwtPayload>());
            _serviceMock.Setup(s => s.AuthService.GetUserIdFromJwtToken(updatePasswordDto.Token)).Returns(userId);
            _serviceMock.Setup(s => s.UserService.UpdatePasswordAsync(userId, updatePasswordDto.Password)).ReturnsAsync(true);

            var actual = await _controller.UpdatePassword(updatePasswordDto);

            _serviceMock.VerifyAll();
            Assert.IsAssignableFrom<NoContentResult>(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task VerifyEmail_TokenIsNullOrEmpty_ReturnsInvalidParameterException(string token)
        {
            var actual = await Assert.ThrowsAnyAsync<Exception>(async() => await _controller.VerifyEmail(token));
            Assert.IsType<InvalidParameterException>(actual);
            Assert.Equal($"Invalid Request Parameter", actual.Message);
        }

        [Fact]
        public async Task VerifyEmail_EmailTokenIsUsed_ReturnsInvalidParameterException()
        {
            string token = "test-token";
            JwtPayload payload = [];
            payload.AddClaim(new(JwtRegisteredClaimNames.Sub, "1"));
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(token)).ReturnsAsync(true);

            var actual = await Assert.ThrowsAnyAsync<Exception>(async () => await _controller.VerifyEmail(token));

            _serviceMock.VerifyAll();
            Assert.IsType<InvalidParameterException>(actual);
            Assert.Equal($"Invalid Link. This link has expired or already been used.", actual.Message);
        }

        [Fact]
        public async Task VerifyEmail_VerifyEmailReturnsFalse_ReturnsInvalidParameterException()
        {
            string token = "test-token";
            JwtPayload payload = [];
            payload.AddClaim(new(JwtRegisteredClaimNames.Sub, "1"));
            _serviceMock.Setup(s => s.AuthService.VerifyJwtToken(token)).Returns(payload);
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(token)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.AuthService.VerifyEmail(1)).ReturnsAsync(false);

            var actual = await Assert.ThrowsAnyAsync<Exception>(async () => await _controller.VerifyEmail(token));
            _serviceMock.VerifyAll();
            Assert.IsType<EmailVerificationFailedException>(actual);
            Assert.Equal($"Something went wrong while Verifying the Email.", actual.Message);
        }

        [Fact]
        public async Task VerifyEmail_VerifyEmailReturnsTrue_ReturnsRedirectResult()
        {
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            JwtPayload payload = [];
            payload.AddClaim(new(JwtRegisteredClaimNames.Sub, "1"));
            _serviceMock.Setup(s => s.AuthService.VerifyJwtToken(token)).Returns(payload);
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(token)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.AuthService.VerifyEmail(1)).ReturnsAsync(true);
            _serviceMock.Setup(s => s.EmailService.RemoveTokenFromCache(token));
            _configurationMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(config.GetSection("FrontendUrl"));

            var actual = await _controller.VerifyEmail(token);

            _serviceMock.VerifyAll();
            Assert.IsType<RedirectResult>(actual);
            Assert.Equal(((RedirectResult)actual).Url, $"{config.GetSection("FrontendUrl").Value}/#/email-verified");
        }

        [Fact]
        public async Task IsEmailVerified_IdIsZero_ReturnsInvalidParameterException()
        {
            int id = 0;
            var actual = await Assert.ThrowsAnyAsync<Exception>(async () => await _controller.IsEmailVerified(id));

            Assert.IsType<InvalidParameterException>(actual);
            Assert.Equal("Invalid Request Parameter", ((InvalidParameterException)actual).Message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsEmailVerified_EmailIsVerifiedSucceed_ReturnsOkObjectResult(bool isEmailVerified)
        {
            int id = 1;
            UserDto userDto = new()
            {
                DisplayName = "Test Name",
                Email = "test@email.com",
                Username = "testuser",
                IsEmailVerified = isEmailVerified
            };
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync(userDto);

            var actual = await _controller.IsEmailVerified(id);

            _serviceMock.VerifyAll();
            Assert.IsType<OkObjectResult>(actual);
            Assert.IsType<bool>(((OkObjectResult)actual).Value);
            bool _actualObject = (bool)((OkObjectResult)actual).Value!;
            Assert.Equal(isEmailVerified, _actualObject);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SendEmailVerification_IdLessThanOne_ReturnsInvalidParameterException(int id)
        {
            var actual = await Assert.ThrowsAnyAsync<InvalidParameterException>(async () => await _controller.SendEmailVerification(id));

            Assert.Equal("Invalid Request Parameter", actual.Message);
        }

        [Fact]
        public async Task SendEmailVerification_EmailAlreadyVerified_ReturnsBadRequest()
        {
            int id = 1;
            UserDto user = new()
            {
                DisplayName = "Test Name",
                Email = "test@email.com",
                Username = "testuser",
                IsEmailVerified = true
            };
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync(user);

            var actual = await _controller.SendEmailVerification(id);

            _serviceMock.VerifyAll();
            Assert.IsType<BadRequestObjectResult>(actual);
            BadRequestObjectResult _actualObject = (BadRequestObjectResult)actual;
            Assert.IsType<string>(_actualObject.Value);
            Assert.Equal($"This Email is already verified", _actualObject.Value);
        }

        [Fact]
        public async Task SendEmailVerification_SendingVerificationEmailFails_ReturnsBadRequest()
        {

            /**
             * cant set the HttpContext.Request property
             */
            int id = 1;
            string token = "test-token";
            UserDto user = new()
            {
                DisplayName = "Test Name",
                Email = "test@email.com",
                Username = "testuser",
                IsEmailVerified = false
            };

            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);

            Assert.Fail();

        }

        [Fact]
        public async Task SendEmailVerification_SendingVerificationEmailSucceeds_NoContent()
        {
            /**
             * cant set the HttpContext.Request property
             */
            int id = 0;
            Assert.Fail();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SendPasswordResetLink_UserIdLessThanOne_ThrowsInvalidParameterException(int userId)
        {
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.SendPasswordResetLink(userId));
            Assert.Equal("Invalid Request Parameter", actual.Message);
        }

        [Fact]
        public async Task SendPasswordResetLink_SendPasswordResetLinkFails_ReturnsBadRequest()
        {
            int userId = 1;
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            UserDto user = new()
            {
                DisplayName = "Test Name",
                Email = "test@email.com",
                Username = "testuser"
            };
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _configurationMock.Setup(c => c.GetSection("FrontendUrl")).Returns(config.GetSection("FrontendUrl"));
            _serviceMock.Setup(s => s.EmailService.SendPasswordResetLink(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var actual = await _controller.SendPasswordResetLink(userId);

            _serviceMock.VerifyAll();
            _configurationMock.VerifyAll();
            Assert.IsType<BadRequestObjectResult>(actual);
            BadRequestObjectResult _actualObject = (BadRequestObjectResult)actual;
            Assert.IsType<string>(_actualObject.Value);
            Assert.Equal("Something went wrong while sending the email.", _actualObject.Value);
        }

        [Fact]
        public async Task SendPasswordResetLink_SendPasswordResetLinkSucceeds_ReturnsNoContent()
        {
            int userId = 1;
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            UserDto user = new()
            {
                DisplayName = "Test Name",
                Email = "test@email.com",
                Username = "testuser"
            };
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _configurationMock.Setup(c => c.GetSection("FrontendUrl")).Returns(config.GetSection("FrontendUrl"));
            _serviceMock.Setup(s => s.EmailService.SendPasswordResetLink(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var actual = await _controller.SendPasswordResetLink(userId);

            _serviceMock.VerifyAll();
            _configurationMock.VerifyAll();
            Assert.IsType<NoContentResult>(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task SendPasswordResetLinkByEmail_EmailIsNullOrEmpty_ThrowsInvalidParameterException(string? email)
        {
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.SendPasswordResetLinkByEmail(email!));
            Assert.Equal("Invalid Request Parameter", actual.Message);
        }

        [Fact]
        public async Task SendPasswordResetLinkByEmail_SendPasswordResetLinkFails_ThrowsInvalidParameterException()
        {
            string email = "test@email.com";
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            UserDto user = new()
            {
                DisplayName = "Test Name",
                Email = "test@email.com",
                Username = "testuser"
            };
            _serviceMock.Setup(s => s.UserService.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _configurationMock.Setup(c => c.GetSection("FrontendUrl")).Returns(config.GetSection("FrontendUrl"));
            _serviceMock.Setup(s => s.EmailService.SendPasswordResetLink(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var actual = await _controller.SendPasswordResetLinkByEmail(email);

            _serviceMock.VerifyAll();
            _configurationMock.VerifyAll();
            Assert.IsType<BadRequestObjectResult>(actual);
            BadRequestObjectResult _actualObject = (BadRequestObjectResult)actual;
            Assert.IsType<string>(_actualObject.Value);
            Assert.Equal("Something went wrong while sending the email.", _actualObject.Value);
        }

        [Fact]
        public async Task SendPasswordResetLinkByEmail_SendPasswordResetLinkSucceeds_ThrowsInvalidParameterException()
        {
            string email = "test@email.com";
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            UserDto user = new()
            {
                DisplayName = "Test Name",
                Email = "test@email.com",
                Username = "testuser"
            };
            _serviceMock.Setup(s => s.UserService.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _configurationMock.Setup(c => c.GetSection("FrontendUrl")).Returns(config.GetSection("FrontendUrl"));
            _serviceMock.Setup(s => s.EmailService.SendPasswordResetLink(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var actual = await _controller.SendPasswordResetLinkByEmail(email);

            _serviceMock.VerifyAll();
            _configurationMock.VerifyAll();
            Assert.IsType<NoContentResult>(actual);
        }

    }
}
