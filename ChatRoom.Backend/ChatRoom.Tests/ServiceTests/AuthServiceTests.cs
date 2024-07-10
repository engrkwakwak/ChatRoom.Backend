using AutoMapper;
using ChatRoom.UnitTest.Helpers;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using RedisCacheService;
using Service;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.IdentityModel.Tokens.Jwt;

namespace ChatRoom.UnitTest.ServiceTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IRepositoryManager> _repositoryMock = new();
        private readonly Mock<ILoggerManager> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IRedisCacheManager> _cacheMock = new();
        private readonly Mock<IFileManager> _fileManagerMock = new();
        private readonly IServiceManager _serviceManager;

        public AuthServiceTests()
        {
            _serviceManager = new ServiceManager(_repositoryMock.Object, _loggerMock.Object, _mapperMock.Object, _configurationMock.Object, _cacheMock.Object, _fileManagerMock.Object);
            //_serviceManager.AuthService.
        }

        [Fact]
        public void CreateEmailVerificationToken_JwtSettingIsEmpty_ThrowsException()
        {
            // arrange
            UserDto userDto = new()
            {
                DisplayName = "Test",
                Email = "test@email.com",
                Username = "testuser"
            };
            User user = new()
            {
                UserId = 1,
                DisplayName = "Test Name",
                Email = "test@email.com",
                PasswordHash = "123",
                Username = "testuser"
            };
            _mapperMock.Setup(m => m.Map<User>(It.IsAny<UserDto>())).Returns(user).Verifiable();

            // act
            var result = Assert.Throws<Exception>(() => _serviceManager.AuthService.CreateEmailVerificationToken(userDto));

            // assert
            _mapperMock.Verify();
            Assert.Equal("Something went wrong while processing the request. Unable generate authentication token.", result.Message);
        }

        [Fact]
        public void CreateEmailVerificationToken_TokenSecretKeyIsNotSet_ThrowsJwtSecretKeyNotFoundException()
        {
            // arrange
            UserDto userDto = new()
            {
                DisplayName = "Test",
                Email = "test@email.com",
                Username = "testuser"
            };
            User user = new()
            {
                UserId = 1,
                DisplayName = "Test Name",
                Email = "test@email.com",
                PasswordHash = "123",
                Username = "testuser"
            };
            _mapperMock.Setup(m => m.Map<User>(It.IsAny<UserDto>())).Returns(user).Verifiable();
            IConfiguration config = ConfigurationFactory.GenerateJwtSettings();
            _configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(config.GetSection("JwtSettings")).Verifiable();
            
            // act
            var result = Assert.Throws<JwtSecretKeyNotFoundException>(() => _serviceManager.AuthService.CreateEmailVerificationToken(userDto));

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            _configurationMock.Verify();
            Assert.Equal("The secret key for jwt token is undefined.", result.Message);
        }

        [Fact]
        public void CreateEmailVerificationToken_Success_ReturnsTokenString()
        {
            // arrange
            UserDto userDto = new()
            {
                DisplayName = "Test",
                Email = "test@email.com",
                Username = "testuser"
            };
            User user = new()
            {
                UserId = 1,
                DisplayName = "Test Name",
                Email = "test@email.com",
                PasswordHash = "123",
                Username = "testuser"
            };
            _mapperMock.Setup(m => m.Map<User>(It.IsAny<UserDto>())).Returns(user).Verifiable();
            IConfiguration config = ConfigurationFactory.GenerateJwtSettings();
            _configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(config.GetSection("JwtSettings")).Verifiable();
            _configurationMock.Setup(c => c["TOKEN_SECRET_KEY"]).Returns(ConfigurationFactory.GetTokenSecretKey()).Verifiable();

            // act 
            var result = _serviceManager.AuthService.CreateEmailVerificationToken(userDto);

            // assert
            _mapperMock.Verify();
            _repositoryMock.Verify();
            _configurationMock.Verify();
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task ValidateUser_UserIsNotFound_ReturnsNull()
        {
            // arrange
            SignInDto userForAuth = new()
            {
                Username = "testusername",
                Password = "password"
            };
            _repositoryMock.Setup(r => r.User.GetUserByUsernameAsync(userForAuth.Username));

            // act
            var result = await _serviceManager.AuthService.ValidateUser(userForAuth);

            // assert
            _loggerMock.Verify(l => l.LogWarn($"{nameof(IAuthService.ValidateUser)}: Authentication failed. Wrong username or password."));
            _repositoryMock.Verify(r => r.User.GetUserByUsernameAsync(userForAuth.Username));
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateUser_CheckPasswordFailedOrFalse_ReturnsNull()
        {
            // arrange
            User user = UserDtoFactory.CreateUser();
            SignInDto userForAuth = new()
            {
                Username = "testusername",
                Password = "passwords"
            };
            _repositoryMock.Setup(r => r.User.GetUserByUsernameAsync(userForAuth.Username)).ReturnsAsync(user).Verifiable();

            // act
            var result = await _serviceManager.AuthService.ValidateUser(userForAuth);

            // assert
            _loggerMock.Verify(l => l.LogWarn($"{nameof(IAuthService.ValidateUser)}: Authentication failed. Wrong username or password."));
            _repositoryMock.Verify(r => r.User.GetUserByUsernameAsync(userForAuth.Username));
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateUser_JwtSettingIsEmpty_ThrowsException()
        {
            // arrange
            User user = UserDtoFactory.CreateUser();
            SignInDto userForAuth = new()
            {
                Username = "testusername",
                Password = "password"
            };
            _repositoryMock.Setup(r => r.User.GetUserByUsernameAsync(userForAuth.Username)).ReturnsAsync(user).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<Exception>( async () => await _serviceManager.AuthService.ValidateUser(userForAuth));

            // assert
            _repositoryMock.Verify();
            Assert.Equal("Something went wrong while processing the request. Unable generate authentication token.", result.Message);
        }

        [Fact]
        public async Task ValidateUser_TokenSecretKeyIsNotSet_ThrowsJwtSecretKeyNotFoundException()
        {
            // arrange
            User user = UserDtoFactory.CreateUser();
            SignInDto userForAuth = new()
            {
                Username = "testusername",
                Password = "password"
            };
            _repositoryMock.Setup(r => r.User.GetUserByUsernameAsync(userForAuth.Username)).ReturnsAsync(user).Verifiable();
            IConfiguration config = ConfigurationFactory.GenerateJwtSettings();
            _configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(config.GetSection("JwtSettings")).Verifiable();

            // act
            var result = await Assert.ThrowsAsync<JwtSecretKeyNotFoundException>(async () => await _serviceManager.AuthService.ValidateUser(userForAuth));

            // assert
            _repositoryMock.Verify();
            _configurationMock.Verify();
            Assert.Equal("The secret key for jwt token is undefined.", result.Message);
        }

        [Fact]
        public async Task ValidateUser_ByUsernameSuccess_ReturnsTokenString()
        {
            // arrange
            User user = UserDtoFactory.CreateUser();
            SignInDto userForAuth = new()
            {
                Username = user.Username,
                Password = "password"
            };
            _repositoryMock.Setup(r => r.User.GetUserByUsernameAsync(userForAuth.Username)).ReturnsAsync(user).Verifiable();
            IConfiguration config = ConfigurationFactory.GenerateJwtSettings();
            _configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(config.GetSection("JwtSettings")).Verifiable();
            _configurationMock.Setup(c => c["TOKEN_SECRET_KEY"]).Returns(ConfigurationFactory.GetTokenSecretKey()).Verifiable();

            // act
            var result = await _serviceManager.AuthService.ValidateUser(userForAuth);

            // assert
            _repositoryMock.Verify();
            _configurationMock.Verify();
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task ValidateUser_ByEmailSuccess_ReturnsTokenString()
        {
            // arrange
            User user = UserDtoFactory.CreateUser();
            SignInDto userForAuth = new()
            {
                Username = user.Email,
                Password = "password"
            };
            _repositoryMock.Setup(r => r.User.GetUserByEmailAsync(userForAuth.Username)).ReturnsAsync(user).Verifiable();
            IConfiguration config = ConfigurationFactory.GenerateJwtSettings();
            _configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(config.GetSection("JwtSettings")).Verifiable();
            _configurationMock.Setup(c => c["TOKEN_SECRET_KEY"]).Returns(ConfigurationFactory.GetTokenSecretKey()).Verifiable();

            // act
            var result = await _serviceManager.AuthService.ValidateUser(userForAuth);

            // assert
            _repositoryMock.Verify();
            _configurationMock.Verify();
            Assert.IsType<string>(result);
        }

        [Fact]
        public void VerifyJwtToken_TokenIsEmpty_ThrowsInvalidParameterException()
        {
            // arrange
            string token = "";

            // act
            var result = Assert.Throws<InvalidParameterException>(() => _serviceManager.AuthService.VerifyJwtToken(token));

            // assert
            Assert.Equal("Invalid Token,the token cannot be empty.", result.Message);
        }

        [Fact]
        public void VerifyJwtToken_TokenIsExpired_ThrowsException()
        {
            // arrange
            string token = UserDtoFactory.GenerateExpiredJwtToken();

            // act
            var result = Assert.Throws<Exception>(() => _serviceManager.AuthService.VerifyJwtToken(token));

            // assert
            _loggerMock.Verify(l => l.LogError($"{nameof(IAuthService.VerifyJwtToken)}: Verification Failed. The token has expired."));
            Assert.Equal("Verification Failed. The token has expired.", result.Message);
        }

        [Fact]
        public void VerifyJwtToken_Success_ReturnsJwtPayload()
        {
            // arrange
            string token = UserDtoFactory.GenerateJwtToken();

            // act
            var result = _serviceManager.AuthService.VerifyJwtToken(token);

            // assert
            Assert.IsType<JwtPayload>(result);
        }

        [Fact]
        public async Task VerifyEmail_VerifyEmailReturnsZero_ReturnsFalse()
        {
            // arrange 
            int userId = 1;
            _repositoryMock.Setup(r => r.User.VerifyEmailAsync(userId)).ReturnsAsync(0).Verifiable();

            // act
            var result = await _serviceManager.AuthService.VerifyEmail(userId);

            // assert
            _repositoryMock.Verify();
            _cacheMock.Verify(cm => cm.RemoveDataAsync($"user:{userId}"));
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyEmail_VerifyEmailReturnsOne_ReturnsFalse()
        {
            // arrange 
            int userId = 1;
            _repositoryMock.Setup(r => r.User.VerifyEmailAsync(userId)).ReturnsAsync(1).Verifiable();

            // act
            var result = await _serviceManager.AuthService.VerifyEmail(userId);

            // assert
            _repositoryMock.Verify();
            _cacheMock.Verify(cm => cm.RemoveDataAsync($"user:{userId}"));
            Assert.True(result);
        }

        [Fact]
        public void GetUserIdFromJwtToken_TokenIsEmpty_ThrowsException()
        {
            // arrange
            string token = "";

            // act 
            var result = Assert.Throws<InvalidParameterException>(() => _serviceManager.AuthService.GetUserIdFromJwtToken(token));

            // assert
            Assert.Equal("Invalid Token,the token cannot be empty.", result.Message);
        }

        [Fact]
        public void GetUserIdFromJwtToken_NoSubClaim_ThrowsException()
        {
            // arrange
            string token = UserDtoFactory.GenerateJwtTokenWithoutSubClaim();

            // act 
            var result = Assert.Throws<Exception>(() => _serviceManager.AuthService.GetUserIdFromJwtToken(token));

            // assert
            Assert.Equal("Invalid token", result.Message);
        }

        [Fact]
        public void GetUserIdFromJwtToken_Default_ReturnsInt()
        {
            int userId = 600;
            User user = UserDtoFactory.CreateUser(userId: userId);
            // arrange
            string token = UserDtoFactory.GenerateJwtToken(user: user);

            // act 
            var result = _serviceManager.AuthService.GetUserIdFromJwtToken(token);

            // assert
            Assert.IsType<int>(result);
            Assert.Equal(userId, result);
        }


    }
}
