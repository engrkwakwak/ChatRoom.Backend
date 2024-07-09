using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Controllers;
using ChatRoom.UnitTest.Helpers;
using Entities.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;

namespace ChatRoom.UnitTest.ControllerTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IServiceManager> _serviceMock = new();

        private readonly Mock<IConfiguration> _configurationMock =  new();

        private readonly AuthController _controller;

        public AuthControllerTests() 
        {
            _controller = new AuthController(_serviceMock.Object, _configurationMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext =  new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public void Authenticate_InvalidInputs_ReturnsValidationError()
        {
            // Arrange
            SignInDto userSignInDto = AuthDtoFactory.CreateInvalidSignInDto();
            var actionArguments = new Dictionary<string, object> { { "user", userSignInDto } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller, 
                nameof(AuthController.SignIn),
                nameof(AuthController),
                actionArguments,
                validationFilter
            );

            // Act
            ModelValidator.ValidateModel(userSignInDto, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;

            // Assert
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);
        }

        [Fact]
        public async Task Authenticate_ValidateUserIsFalse_ReturnsUnauthorizedStatus()
        {
            // Arrange
            SignInDto userSignInDto = AuthDtoFactory.CreateValidSignInDto();
            _serviceMock.Setup(s => s.AuthService.ValidateUser(userSignInDto)).ReturnsAsync(false);

            // Act
            var actual = await _controller.Authenticate(userSignInDto);
            
            // Assert
            _serviceMock.Verify(m => m.AuthService.ValidateUser(userSignInDto), Times.Once);
            Assert.IsType<UnauthorizedResult>(actual);
        }

        [Fact]
        public async Task Authenticate_ValidateUserIsTrue_ReturnsOkStatus()
        {
            // Arrange
            SignInDto signInDto = AuthDtoFactory.CreateValidSignInDto();
            _serviceMock.Setup(s => s.AuthService.ValidateUser(signInDto)).ReturnsAsync(true);

            // Act
            var actual = await _controller.Authenticate(signInDto) as ObjectResult;

            // Assert
            _serviceMock.Verify(m => m.AuthService.ValidateUser(signInDto), Times.Once);
            Assert.IsType<OkObjectResult>(actual);
            Assert.Equal(200, actual.StatusCode);
        }

        [Fact]  
        public async Task SignUp_HasDuplicateEmail_ReturnsValidationException()
        {
            // Arrange
            SignUpDto signUpDto = AuthDtoFactory.CreateValidSignUpDto();
            _serviceMock.Setup(s => s.UserService.HasDuplicateEmailAsync(signUpDto.Email!)).ReturnsAsync(true);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<ValidationException>(async () => await _controller.SignUp(signUpDto));
            _serviceMock.Verify(m => m.UserService.HasDuplicateEmailAsync(signUpDto.Email!), Times.Once);
            Assert.Equal($"The email {signUpDto.Email} is already in used by another user.", actual.Message);
        }

        [Fact]  
        public async Task SignUp_HasDuplicateUsername_ReturnsValidationException()
        {
            // Arrange
            SignUpDto signUpDto = AuthDtoFactory.CreateValidSignUpDto();
            _serviceMock.Setup(s => s.UserService.HasDuplicateEmailAsync(signUpDto.Email!)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.UserService.HasDuplicateUsernameAsync(signUpDto.Username!)).ReturnsAsync(true);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<ValidationException>(async () => await _controller.SignUp(signUpDto));
            _serviceMock.VerifyAll();
            Assert.Equal($"The username {signUpDto.Username} is already in used by another user.", actual.Message);
        }

        [Fact]  
        public async Task SignUp_PasswordsDontMatch_ReturnsValidationException()
        {
            // Arrange
            SignUpDto signUpDto = AuthDtoFactory.CreateValidSignUpDto();
            signUpDto.PasswordConfirmation = $"{signUpDto.Password}{signUpDto.Password}";
            _serviceMock.Setup(s => s.UserService.HasDuplicateEmailAsync(signUpDto.Email!)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.UserService.HasDuplicateUsernameAsync(signUpDto.Username!)).ReturnsAsync(false);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<ValidationException>(async () => await _controller.SignUp(signUpDto));
            _serviceMock.VerifyAll();
            Assert.Equal($"The Password and Password Confirmation didnt match.", actual.Message);
        }

        [Fact]
        public void SignUp_InvalidData_ReturnsUnprocessableEntity()
        {
            SignUpDto signUpDto = AuthDtoFactory.CreateInvalidSignUpDto();
            var actionArguments = new Dictionary<string, object> { { "signUpData", signUpDto } };
            var validationFilter = new ValidationFilterAttribute();
            var actionExecutingContext = FilterTestHelper.CreateActionExecutingContext(
                _controller,
                nameof(AuthController.SignUp),
                nameof(AuthController),
                actionArguments,
                validationFilter
            );

            // Act
            ModelValidator.ValidateModel(signUpDto, _controller, actionExecutingContext);
            FilterTestHelper.InvokeActionFilter(validationFilter, actionExecutingContext);
            var filterResult = actionExecutingContext.Result as ObjectResult;
            
            Assert.NotNull(filterResult);
            Assert.IsType<UnprocessableEntityObjectResult>(filterResult);
            Assert.Equal(422, filterResult.StatusCode);

        }

        [Fact]
        public async Task SignUp_SendVerificationEmailIsFalse_ReturnsBadRequest()
        {
            // Arrange
            SignUpDto user = AuthDtoFactory.CreateValidSignUpDto();
            UserDto createdUser = new()
            {
                Username = user.Username!,
                DisplayName = user.DisplayName!,
                Email = user.Email!,
                UserId = 1
            };
            _controller.HttpContext.Request.Scheme = "http";
            _controller.HttpContext.Request.Host = HostString.FromUriComponent("test-host");
            _serviceMock.Setup(s => s.UserService.HasDuplicateEmailAsync(user.Email!)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.UserService.HasDuplicateUsernameAsync(user.Username!)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.UserService.InsertUserAsync(user)).ReturnsAsync(createdUser);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(createdUser)).Returns("test-token");
            _serviceMock.Setup(s => s.EmailService.SendVerificationEmail(createdUser, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<EmailNotSentException>(async () => await _controller.SignUp(user) );
            _serviceMock.VerifyAll();
            Assert.Equal($"Something went wrong while sending the verification email.", actual.Message);
        }

        [Fact]
        public async Task UpdatePassword_PasswordsDoesntMatch_ReturnsValidationException()
        {
            // Arrange
            UpdatePasswordDto updatePasswordDto = new()
            {
                Password = "password",
                PasswordConfirmation = "password_",
                Token = ""
            };

            // Act & Assert
            var actual = await Assert.ThrowsAsync<ValidationException>(async () => await _controller.UpdatePassword(updatePasswordDto));
            Assert.Equal("The passwords doesnt match.", actual.Message);
        }

        [Fact]
        public async Task UpdatePassword_UpdatePasswordFailed_ReturnsUserUpdateFailedException()
        {
            // Arrange
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

            // Act & Assert
            var actual = await Assert.ThrowsAsync<UserUpdateFailedException>(async () => await _controller.UpdatePassword(updatePasswordDto));
            _serviceMock.VerifyAll();
        }
          

        [Fact]
        public async Task UpdatePassword_EmailTokenIsUsed_ReturnsInvalidParameterException()
        {
            // Arrange
            UpdatePasswordDto updatePasswordDto = new()
            {
                Password = "password",
                PasswordConfirmation = "password",
                Token = It.IsAny<string>(),
            };
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(It.IsAny<string>())).ReturnsAsync(true);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.UpdatePassword(updatePasswordDto) );
            _serviceMock.VerifyAll();
            Assert.Equal($"Invalid Link. This link has expired or already been used.", actual.Message);
        }

        [Fact]
        public async Task UpdatePassword_UpdatePasswordSucceeds_ReturnsNoContent()
        {
            // Arrange
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

            // Act
            var actual = await _controller.UpdatePassword(updatePasswordDto);

            // Assert
            _serviceMock.VerifyAll();
            Assert.IsAssignableFrom<NoContentResult>(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task VerifyEmail_TokenIsNullOrEmpty_ReturnsInvalidParameterException(string token)
        {
            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async() => await _controller.VerifyEmail(token));
            Assert.Equal($"Invalid Request Parameter", actual.Message);
        }

        [Fact]
        public async Task VerifyEmail_EmailTokenIsUsed_ReturnsInvalidParameterException()
        {
            // Arrange
            string token = "test-token";
            JwtPayload payload = [];
            payload.AddClaim(new(JwtRegisteredClaimNames.Sub, "1"));
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(token)).ReturnsAsync(true);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.VerifyEmail(token));
            _serviceMock.VerifyAll();
            Assert.Equal($"Invalid Link. This link has expired or already been used.", actual.Message);
        }

        [Fact]
        public async Task VerifyEmail_VerifyEmailReturnsFalse_ReturnsInvalidParameterException()
        {
            // Arrange
            string token = "test-token";
            JwtPayload payload = [];
            payload.AddClaim(new(JwtRegisteredClaimNames.Sub, "1"));
            _serviceMock.Setup(s => s.AuthService.VerifyJwtToken(token)).Returns(payload);
            _serviceMock.Setup(s => s.EmailService.IsEmailTokenUsed(token)).ReturnsAsync(false);
            _serviceMock.Setup(s => s.AuthService.VerifyEmail(1)).ReturnsAsync(false);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<EmailVerificationFailedException>(async () => await _controller.VerifyEmail(token));
            _serviceMock.VerifyAll();
            Assert.Equal($"Something went wrong while Verifying the Email.", actual.Message);
        }

        [Fact]
        public async Task VerifyEmail_VerifyEmailReturnsTrue_ReturnsRedirectResult()
        {
            // Arrange
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

            // Act
            var actual = await _controller.VerifyEmail(token);

            // Assert
            _serviceMock.VerifyAll();
            Assert.IsType<RedirectResult>(actual);
            Assert.Equal(((RedirectResult)actual).Url, $"{config.GetSection("FrontendUrl").Value}/#/email-verified");
        }

        [Fact]
        public async Task IsEmailVerified_IdIsZero_ReturnsInvalidParameterException()
        {
            // Arrange
            int id = 0;

            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.IsEmailVerified(id));
            Assert.Equal("Invalid Request Parameter", ((InvalidParameterException)actual).Message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsEmailVerified_EmailIsVerifiedSucceed_ReturnsOkObjectResult(bool isEmailVerified)
        {
            // Arrange
            int id = 1;
            UserDto userDto = UserDtoFactory.CreateUserDto();
            userDto.IsEmailVerified = isEmailVerified;
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync(userDto);

            // Act
            var actual = await _controller.IsEmailVerified(id);

            // Assert
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
            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.SendEmailVerification(id));
            Assert.Equal("Invalid Request Parameter", actual.Message);
        }

        [Fact]
        public async Task SendEmailVerification_EmailAlreadyVerified_ReturnsBadRequest()
        {
            // Arrange
            int id = 1;
            UserDto user = UserDtoFactory.CreateUserDto();
            user.IsEmailVerified = true;
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync(user);

            // Act
            var actual = await _controller.SendEmailVerification(id);

            // Assert
            _serviceMock.VerifyAll();
            Assert.IsType<BadRequestObjectResult>(actual);
            BadRequestObjectResult _actualObject = (BadRequestObjectResult)actual;
            Assert.IsType<string>(_actualObject.Value);
            Assert.Equal($"This Email is already verified", _actualObject.Value);
        }

        [Fact]
        public async Task SendEmailVerification_SendingVerificationEmailFails_ReturnsBadRequest()
        {
            // Arrange
            int id = 1;
            string token = "test-token";
            UserDto user = UserDtoFactory.CreateUserDto();
            user.IsEmailVerified = false;
            _controller.HttpContext.Request.Scheme = "http";
            _controller.HttpContext.Request.Host = HostString.FromUriComponent("test-host");
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _serviceMock.Setup(s => s.EmailService.SendVerificationEmail(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var actual = await _controller.SendEmailVerification(id);

            // Assert
            _serviceMock.VerifyAll();
            Assert.IsType<BadRequestObjectResult>(actual);
            BadRequestObjectResult _actualObject = (BadRequestObjectResult)actual;
            Assert.IsType<string>(_actualObject.Value);
            Assert.Equal($"Something went wrong while sending the email.", _actualObject.Value);
        }

        [Fact]
        public async Task SendEmailVerification_SendingVerificationEmailSucceeds_NoContent()
        {
            // Arrange
            int id = 1;
            string token = "test-token";
            UserDto user = UserDtoFactory.CreateUserDto();
            user.IsEmailVerified = false;
            _controller.HttpContext.Request.Scheme = "http";
            _controller.HttpContext.Request.Host = HostString.FromUriComponent("test-host");
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(id)).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _serviceMock.Setup(s => s.EmailService.SendVerificationEmail(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var actual = await _controller.SendEmailVerification(id);

            // Assert
            _serviceMock.VerifyAll();
            Assert.IsType<NoContentResult>(actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SendPasswordResetLink_UserIdLessThanOne_ThrowsInvalidParameterException(int userId)
        {
            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.SendPasswordResetLink(userId));
            Assert.Equal("Invalid Request Parameter", actual.Message);
        }

        [Fact]
        public async Task SendPasswordResetLink_SendPasswordResetLinkFails_ReturnsBadRequest()
        {
            // Arrange
            int userId = 1;
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            UserDto user = UserDtoFactory.CreateUserDto();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _configurationMock.Setup(c => c.GetSection("FrontendUrl")).Returns(config.GetSection("FrontendUrl"));
            _serviceMock.Setup(s => s.EmailService.SendPasswordResetLink(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var actual = await _controller.SendPasswordResetLink(userId);

            // Arrange
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
            // Arrange
            int userId = 1;
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            UserDto user = UserDtoFactory.CreateUserDto();
            _serviceMock.Setup(s => s.UserService.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _configurationMock.Setup(c => c.GetSection("FrontendUrl")).Returns(config.GetSection("FrontendUrl"));
            _serviceMock.Setup(s => s.EmailService.SendPasswordResetLink(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var actual = await _controller.SendPasswordResetLink(userId);

            // Assert
            _serviceMock.VerifyAll();
            _configurationMock.VerifyAll();
            Assert.IsType<NoContentResult>(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task SendPasswordResetLinkByEmail_EmailIsNullOrEmpty_ThrowsInvalidParameterException(string? email)
        {
            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidParameterException>(async () => await _controller.SendPasswordResetLinkByEmail(email!));
            Assert.Equal("Invalid Request Parameter", actual.Message);
        }

        [Fact]
        public async Task SendPasswordResetLinkByEmail_SendPasswordResetLinkFails_ThrowsInvalidParameterException()
        {
            // Arrange
            string email = "test@email.com";
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            UserDto user = UserDtoFactory.CreateUserDto();
            _serviceMock.Setup(s => s.UserService.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _configurationMock.Setup(c => c.GetSection("FrontendUrl")).Returns(config.GetSection("FrontendUrl"));
            _serviceMock.Setup(s => s.EmailService.SendPasswordResetLink(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var actual = await _controller.SendPasswordResetLinkByEmail(email);

            // Assert
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
            // Arrange
            string email = "test@email.com";
            string token = "test-token";
            var inMemorySettings = new Dictionary<string, string?> {
                {"FrontendUrl", "frontend-url"},
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            UserDto user = UserDtoFactory.CreateUserDto();
            _serviceMock.Setup(s => s.UserService.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _serviceMock.Setup(s => s.AuthService.CreateEmailVerificationToken(user)).Returns(token);
            _configurationMock.Setup(c => c.GetSection("FrontendUrl")).Returns(config.GetSection("FrontendUrl"));
            _serviceMock.Setup(s => s.EmailService.SendPasswordResetLink(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var actual = await _controller.SendPasswordResetLinkByEmail(email);


            // Assert
            _serviceMock.VerifyAll();
            _configurationMock.VerifyAll();
            Assert.IsType<NoContentResult>(actual);
        }

    }
}
