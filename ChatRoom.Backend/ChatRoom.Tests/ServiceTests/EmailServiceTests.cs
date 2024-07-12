using AutoMapper;
using ChatRoom.Backend;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using RedisCacheService;
using Service;
using Shared.DataTransferObjects.Email;
using Shared.DataTransferObjects.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ChatRoom.UnitTest.ServiceTests;
public class EmailServiceTests {
    private readonly Mock<IRedisCacheManager> _mockCache;
    private readonly Mock<ISmtpClientManager> _mockSmtp;
    private readonly EmailService _service;

    public EmailServiceTests() {
        _mockCache = new Mock<IRedisCacheManager>();
        _mockSmtp = new Mock<ISmtpClientManager>();

        var mappingConfig = new MapperConfiguration(mc => {
            mc.AddProfile(new MappingProfile());
        });
        var mapper = mappingConfig.CreateMapper();

        _service = new EmailService(_mockCache.Object, _mockSmtp.Object);
    }

    [Fact]
    public async Task SendVerificationEmail_ShouldStoreTheTokenInCache() {
        // Arrange
        UserDto user = CreateUserDto();
        string token = GenerateJwtToken();
        string verificationLink = "sampleLink";

        // Act
        await _service.SendVerificationEmail(user, verificationLink, token);

        // Assert
        _mockCache.Verify(x => x.SetCachedDataWithAbsoluteExpAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task SendVerificationEmail_SmtpConfigurationDoesNotExistInConfiguration_ShouldThrowNotFoundException() {
        // Arrange
        UserDto user = CreateUserDto();
        string token = GenerateJwtToken();
        string verificationLink = "sampleLink";

        _mockSmtp.Setup(x => x.SendEmailAsync(It.IsAny<EmailDto>()))
            .ThrowsAsync(new SmptConfigurationNotFoundException("SMTPConfiguration"));

        // Act
        Func<Task> act = async() => await _service.SendVerificationEmail(user, verificationLink, token);

        // Assert
        await act.Should().ThrowAsync<SmptConfigurationNotFoundException>()
            .WithMessage("Email configuration with name SMTPConfiguration does not exists in configurations.");
    }

    [Fact]
    public async Task SendVerificationEmail_SmtpClientSuccessfullySentTheEmail_ShouldReturnTrue() {
        // Arrange
        UserDto userDto = CreateUserDto();
        string token = GenerateJwtToken();
        string verificationLink = "sampleLink";

        _mockSmtp.Setup(x => x.SendEmailAsync(It.IsAny<EmailDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.SendVerificationEmail(userDto, verificationLink, token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendVerificationEmail_SmtpClientFailedToSendTheEmail_ShouldReturnFalse() {
        // Arrange
        UserDto userDto = CreateUserDto();
        string token = GenerateJwtToken();
        string verificationLink = "sampleLink";

        _mockSmtp.Setup(x => x.SendEmailAsync(It.IsAny<EmailDto>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.SendVerificationEmail(userDto, verificationLink, token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendPasswordResetLink_ShouldStoreTheTokenInCache() {
        // Arrange
        UserDto user = CreateUserDto();
        string token = GenerateJwtToken();
        string passwordResetLink = "sampleLink";

        // Act
        await _service.SendPasswordResetLink(user, passwordResetLink, token);

        // Assert
        _mockCache.Verify(x => x.SetCachedDataWithAbsoluteExpAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetLink_SmtpConfigurationDoesNotExistInConfiguration_ShouldThrowNotFoundException() {
        // Arrange
        UserDto user = CreateUserDto();
        string token = GenerateJwtToken();
        string verificationLink = "sampleLink";

        _mockSmtp.Setup(x => x.SendEmailAsync(It.IsAny<EmailDto>()))
            .ThrowsAsync(new SmptConfigurationNotFoundException("SMTPConfiguration"));

        // Act
        Func<Task> act = async () => await _service.SendVerificationEmail(user, verificationLink, token);

        // Assert
        await act.Should().ThrowAsync<SmptConfigurationNotFoundException>()
            .WithMessage("Email configuration with name SMTPConfiguration does not exists in configurations.");
    }

    [Fact]
    public async Task SendPasswordResetLink_SmtpClientSuccessfullySentTheEmail_ShouldReturnTrue() {
        // Arrange
        UserDto userDto = CreateUserDto();
        string token = GenerateJwtToken();
        string verificationLink = "sampleLink";

        _mockSmtp.Setup(x => x.SendEmailAsync(It.IsAny<EmailDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.SendPasswordResetLink(userDto, verificationLink, token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendPasswordResetLink_SmtpClientFailedToSendTheEmail_ShouldReturnFalse() {
        // Arrange
        UserDto userDto = CreateUserDto();
        string token = GenerateJwtToken();
        string verificationLink = "sampleLink";

        _mockSmtp.Setup(x => x.SendEmailAsync(It.IsAny<EmailDto>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.SendPasswordResetLink(userDto, verificationLink, token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveTokenFromCache_ShouldRemoveTokenFromCache() {
        // Arrange
        string token = "sampleToken";
        string? capturedKey = null;

        _mockCache.Setup(x => x.RemoveDataAsync(It.IsAny<string>()))
            .Callback<string>(k => capturedKey = k);

        // Act
        await _service.RemoveTokenFromCache(token);

        // Assert
        capturedKey.Should().NotBeNull();
        capturedKey.Should().Be($"email-{token}");

        _mockCache.Verify(x => x.RemoveDataAsync(capturedKey!), Times.Once());
    }

    [Fact]
    public async Task IsEmailTokenUsed_TokenDoesNotExistInCache_ShouldReturnTrue() {
        // Arrange
        string token = "sampleToken";

        _mockCache.Setup(x => x.GetCachedDataAsync<string?>(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.IsEmailTokenUsed($"email-{token}");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailTokenUsed_TokenExistsInCache_ShouldReturnFalse() {
        // Arrange
        string token = "sampleToken";

        _mockCache.Setup(x => x.GetCachedDataAsync<string?>(It.IsAny<string>()))
            .ReturnsAsync(token);

        // Act
        var result = await _service.IsEmailTokenUsed($"email-{token}");

        // Assert
        result.Should().BeFalse();
    }

    private static UserDto CreateUserDto() {
        return new UserDto {
            DisplayName = "Test",
            Email = "te@st.com",
            Username = "test",
            UserId = 1
        };
    }

    private static User CreateUser() {
        return new User() {
            DisplayName = "Test",
            Email = "test@test.com",
            PasswordHash = "$2a$11$wz1mmSBhCfO.AJI4Ll8qZ.KQvqob3mQhN28F7SqB46XLdizFmYYX6",
            Username = "test",
            UserId = 1,
            IsEmailVerified = true,
            DisplayPictureUrl = "currentPictureUrl"
        };
    }

    private static string GenerateJwtToken(string tokenSecretKey = "cHaTrOoM-sEcReTkEy-1$2$3$4$5$6$7$8$9$0$-GREATERTHAN256BYTES", User? user = null) {
        User _user = user ?? CreateUser();
        List<Claim> claims = [
            new(JwtRegisteredClaimNames.Sub, _user!.UserId.ToString()),
                new("display-name", _user.DisplayName),
                new("display-picture", _user.DisplayPictureUrl ?? ""),
                new(ClaimTypes.NameIdentifier, _user.UserId.ToString())
        ];
        byte[] key = Encoding.UTF8.GetBytes(tokenSecretKey);
        SigningCredentials signingCredentials = new(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256
        );
        JwtSecurityToken tokenOptions = new(
            issuer: "validIssuer",
            audience: "validAudience",
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }
}
