using Service;
using Moq;
using Contracts;
using AutoMapper;
using RedisCacheService;
using Entities.Models;
using ChatRoom.Backend;
using Shared.DataTransferObjects.Users;
using Entities.Exceptions;
using Shared.DataTransferObjects.Auth;
using Shared.RequestFeatures;
using FluentAssertions;

namespace ChatRoom.UnitTest.ServiceTests; 
public class UserServiceTests {
    private readonly Mock<IRepositoryManager> _mockRepo;
    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly Mock<IRedisCacheManager> _mockCache;
    private readonly Mock<IFileManager> _mockFileManager;
    private readonly IMapper _mapper;
    private readonly UserService _service;

    public UserServiceTests() {
        _mockRepo = new Mock<IRepositoryManager>();
        _mockLogger = new Mock<ILoggerManager>();
        _mockCache = new Mock<IRedisCacheManager>();
        _mockFileManager = new Mock<IFileManager>();

        var mappingConfig = new MapperConfiguration(mc => {
            mc.AddProfile(new MappingProfile());
        });
        _mapper = mappingConfig.CreateMapper();

        _service = new UserService(
            _mockRepo.Object, _mockLogger.Object, _mapper,
            _mockCache.Object, _mockFileManager.Object
        );
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExistsInCache_ShouldReturnUserDto() {
        // Arrange
        User userFromCache = CreateUser();

        _mockCache.Setup(x => x.GetCachedDataAsync<User>(It.IsAny<string>()))
            .ReturnsAsync(userFromCache);

        // Act
        var result = await _service.GetUserByIdAsync(userFromCache.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<UserDto>(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExistsInCache_ShouldMapUserToUserDtoCorrectly() {
        // Arrange
        User userFromCache = CreateUser();

        _mockCache.Setup(x => x.GetCachedDataAsync<User>(It.IsAny<string>()))
            .ReturnsAsync(userFromCache);

        // Act
        var result = await _service.GetUserByIdAsync(userFromCache.UserId);

        // Assert
        Assert.Equal(userFromCache.UserId, result.UserId);
        Assert.Equal(userFromCache.Username, result.Username);
        Assert.Equal(userFromCache.DisplayName, result.DisplayName);
        Assert.Equal(userFromCache.Email, result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExistsInCache_GetUserFromDatabaseNotExecuted() {
        // Arrange
        User userFromCache = CreateUser();

        _mockCache.Setup(x => x.GetCachedDataAsync<User>(It.IsAny<string>()))
            .ReturnsAsync(userFromCache);

        // Act
        var result = await _service.GetUserByIdAsync(userFromCache.UserId);

        // Verify
        _mockRepo.Verify(x => x.User.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExistsInDatabase_ShouldReturnUserDto() {
        // Arrange
        User userFromDatabase = CreateUser();

        _mockCache.Setup(x => x.GetCachedDataAsync<User?>(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(userFromDatabase);

        // Act
        var result = await _service.GetUserByIdAsync(userFromDatabase.UserId);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<UserDto>(result);
    }

    [Fact]
    public async Task GetuserByIdAsync_UserExistsInDatabase_ShouldMapUserToUserDtoCorrectly() {
        // Arrange
        User userFromDatabase = CreateUser();

        _mockCache.Setup(x => x.GetCachedDataAsync<User?>(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(userFromDatabase);

        // Act
        var result = await _service.GetUserByIdAsync(userFromDatabase.UserId);

        // Assert
        Assert.Equal(userFromDatabase.UserId, result.UserId);
        Assert.Equal(userFromDatabase.Username, result.Username);
        Assert.Equal(userFromDatabase.DisplayName, result.DisplayName);
        Assert.Equal(userFromDatabase.Email, result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExistsInDatabase_ShouldAddUserToCache() {
        // Arrange
        User userFromDatabase = CreateUser();

        _mockCache.Setup(x => x.GetCachedDataAsync<User?>(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(userFromDatabase);

        // Act
        var result = await _service.GetUserByIdAsync(userFromDatabase.UserId);

        // Verify
        _mockCache.Verify(x => x.SetCachedDataAsync<User>($"user:{userFromDatabase.UserId}", It.IsAny<User>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserDoesNotExistInDatabase_ShouldThrowNotFoundException() {
        // Arrange
        int userId = 1;
        string expectedErrMsg = $"The user with id: {userId} doesn't exists in the database.";

        _mockCache.Setup(x => x.GetCachedDataAsync<User?>(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var err = await Assert.ThrowsAsync<UserIdNotFoundException>(() => _service.GetUserByIdAsync(userId));

        Assert.Equal(expectedErrMsg, err.Message);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserDoesNotExistInDatabase_SetCacheDataAsyncNotExecuted() {
        // Arrange
        int userId = 1;

        _mockCache.Setup(x => x.GetCachedDataAsync<User?>(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var err = await Assert.ThrowsAsync<UserIdNotFoundException>(() => _service.GetUserByIdAsync(userId));

        // Verify
        _mockCache.Verify(x => x.SetCachedDataAsync<User>($"user:{userId}", It.IsAny<User>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task HasDuplicateEmailAsync_EmailExistsInDatabase_ShouldReturnTrue() {
        // Arrange
        string email = "sam@ple.com";

        _mockRepo.Setup(x => x.User.HasDuplicateEmailAsync(email))
            .ReturnsAsync(1);

        // Act
        var result = await _service.HasDuplicateEmailAsync(email);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasDuplicateEmailAsync_EmailDoesNotExistInDatabase_ShouldReturnFalse() {
        // Arrange
        string email = "sam@ple.com";

        _mockRepo.Setup(x => x.User.HasDuplicateEmailAsync(email))
            .ReturnsAsync(0);

        // Act
        var result = await _service.HasDuplicateEmailAsync(email);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasDuplicateUsernameAsync_UsernameExistsInDatabase_ShouldReturnTrue() {
        // Arrange
        string username = "sample";

        _mockRepo.Setup(x => x.User.HasDuplicateUsernameAsync(username))
            .ReturnsAsync(1);

        // Act
        var result = await _service.HasDuplicateUsernameAsync(username);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasDuplicateUsernameAsync_UsernameDoesNotExistInDatabase_ShouldReturnFalse() {
        // Arrange
        string username = "sample";

        _mockRepo.Setup(x => x.User.HasDuplicateUsernameAsync(username))
            .ReturnsAsync(0);

        // Act
        var result = await _service.HasDuplicateUsernameAsync(username);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InsertUserAsync_ValidSignUpData_ShouldMapSignUpDtoToUserCorrectly() {
        // Arrange
        SignUpDto signUpData = CreateSignUpDto();

        _mockRepo.Setup(r => r.User.InsertUserAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _service.InsertUserAsync(signUpData);

        // Assert
        Assert.Equal(signUpData.Username, result.Username);
        Assert.Equal(signUpData.DisplayName, result.DisplayName);
        Assert.Equal(signUpData.Email, result.Email);
    }

    [Fact]
    public async Task InsertUserAsync_ValidSignUpData_ShouldHashPasswordCorrectly() {
        // Arrange
        SignUpDto signUpData = CreateSignUpDto();

        User? capturedUser = null;
        _mockRepo.Setup(r => r.User.InsertUserAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _service.InsertUserAsync(signUpData);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify(signUpData.Password, capturedUser.PasswordHash));
    }

    [Fact]
    public async Task InsertUserAsync_RepositoryInsertFails_ShouldThrowException() {
        // Arrange
        SignUpDto signUpData = CreateSignUpDto();

        _mockRepo.Setup(r => r.User.InsertUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act & Assert
        var err = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.InsertUserAsync(signUpData));
    }

    [Fact]
    public async Task InsertUserAsync_ValidSignUpData_ShouldReturnUserDto() {
        // Arrange
        SignUpDto signUpData = CreateSignUpDto();

        _mockRepo.Setup(r => r.User.InsertUserAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _service.InsertUserAsync(signUpData);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<UserDto>(result);
    }

    [Fact]
    public async Task UpdateUserAsync_UserDoesNotExistInDatabase_ShouldThrowNotFoundException() {
        // Arrange
        int userId = 1;
        string expectedErrMsg = $"The user with id: {userId} doesn't exists in the database.";

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new UserIdNotFoundException(userId));

        // Act & Assert
        var err = await Assert.ThrowsAsync<UserIdNotFoundException>(() => _service.GetUserByIdAsync(userId));

        Assert.Equal(expectedErrMsg, err.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_EmailChanged_ShouldSetIsEmailVerifiedToFalse() {
        // Arrange
        int userId = 1;
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        User user = CreateUser();

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserAsync(userId, userForUpdate);

        // Assert
        Assert.False(user.IsEmailVerified);
    }

    [Fact]
    public async Task UpdateUserAsync_EmailRemainUnchanged_ShouldKeepTheCurrentVerifiedValue() {
        // Arrange
        int userId = 1;
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        userForUpdate.Email = user.Email;

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserAsync(userId, userForUpdate);

        // Assert
        Assert.True(user.IsEmailVerified);
    }

    [Fact]
    public async Task UpdateUserAsync_ProfileImageIsChanged_ShouldDeleteOldPicture() {
        // Arrange
        int userId = 1;
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserAsync(userId, userForUpdate);

        // Verify
        _mockFileManager.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ProfileImageRemainUnchanged_ShouldNotDeleteOldPicture() {
        // Arrange
        int userId = 1;
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        userForUpdate.DisplayPictureUrl = user.DisplayPictureUrl;

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserAsync(userId, userForUpdate);

        // Verify
        _mockFileManager.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_CurrentlyHasNoPicture_ShouldNotDeleteOldPicture() {
        // Arrange
        int userId = 1;
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        user.DisplayPictureUrl = null;

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserAsync(userId, userForUpdate);

        // Verify
        _mockFileManager.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_CurrentlyHasNoPictureAndNoNewPicture_ShouldNotDeleteOldPicture() {
        // Arrange
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        user.DisplayPictureUrl = null;
        userForUpdate.DisplayPictureUrl = null;

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserAsync(user.UserId, userForUpdate);

        // Verify
        _mockFileManager.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_RepositoryUpdateFails_ShouldThrowUserUpdateFailedException() {
        // Arrange
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        string expectedErrMsg = $"The server failed to update the user with id: {user.UserId}.";

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(0);

        // Act & Assert
        var err = await Assert.ThrowsAsync<UserUpdateFailedException>(() => _service.UpdateUserAsync(user.UserId, userForUpdate));

        Assert.Equal(expectedErrMsg, err.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_RepositoryUpdateFails_ShouldLogWarning() {
        // Arrange
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        string expectedErrMsg = $"The server failed to update the user with id: {user.UserId}.";

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(0);

        // Act & Assert
        var err = await Assert.ThrowsAsync<UserUpdateFailedException>(() => _service.UpdateUserAsync(user.UserId, userForUpdate));

        // Verify
        _mockLogger.Verify(x => x.LogWarn(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ValidUserAndUpdateData_ShouldUpdateUserSuccessfully() {
        // Arrange
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        string expectedErrMsg = $"The server failed to update the user with id: {user.UserId}.";

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserAsync(user.UserId, userForUpdate);

        // Assert & Verify
        _mockRepo.Verify(x => x.User.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ValidUserAndUpdateData_ShouldRemoveUserFromCache() {
        // Arrange
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        string expectedErrMsg = $"The server failed to update the user with id: {user.UserId}.";

        _mockRepo.Setup(x => x.User.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(user);
        _mockRepo.Setup(x => x.User.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserAsync(user.UserId, userForUpdate);

        // Assert & Verify
        _mockCache.Verify(x => x.RemoveDataAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_RepositoryReturnsEmptyList_ShouldReturnEmptyUsersAndMetadata() {
        // Arrange
        PagedList<User> usersWithMetaData = new([], 0, 1, 10);
        UserParameters userParameters = new();

        _mockRepo.Setup(x => x.User.GetUsersAsync(It.IsAny<UserParameters>()))
            .ReturnsAsync(usersWithMetaData);

        // Act
        var (users, metaData) = await _service.GetUsersAsync(userParameters);

        // Assert
        Assert.NotNull(users);
        Assert.Empty(users);
        Assert.NotNull(metaData);
    }

    [Fact]
    public async Task GetUsersAsync_ValidUserParameters_ShouldReturnUsersAndMetaData() {
        // Arrange
        PagedList<User> usersWithMetaData = new(CreateUsers(), 0, 1, 10);
        UserParameters userParameters = new();

        _mockRepo.Setup(x => x.User.GetUsersAsync(It.IsAny<UserParameters>()))
            .ReturnsAsync(usersWithMetaData);

        // Act
        var (users, metaData) = await _service.GetUsersAsync(userParameters);

        // Assert
        Assert.NotNull(users);
        Assert.Equal(usersWithMetaData.Count, users.Count());
        Assert.NotNull(metaData);
    }

    [Fact]
    public async Task GetUsersAsync_ValidUserParameters_ShouldMapUsersToUserDtosCorrectly() {
        // Arrange
        PagedList<User> usersWithMetaData = new(CreateUsers(), 0, 1, 10);
        UserParameters userParameters = new();

        _mockRepo.Setup(x => x.User.GetUsersAsync(It.IsAny<UserParameters>()))
            .ReturnsAsync(usersWithMetaData);

        // Act
        (IEnumerable<UserDto> users, MetaData? metaData) = await _service.GetUsersAsync(userParameters);

        // Assert
        users.Should().BeEquivalentTo(usersWithMetaData, options => options
            .Excluding(m => m.PasswordHash)
            .Excluding(m => m.Contacts)
            .Excluding(m => m.ParticipatedChats)
            .Excluding(m => m.Messages)
        );
    }

    [Fact]
    public async Task UpdatePasswordAsync_RepositoryUpdateFails_ShouldReturnFalse() {
        // Arrange
        int userId = 1;
        string password = "password";

        _mockRepo.Setup(x => x.User.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(-1);

        // Act
        var result = await _service.UpdatePasswordAsync(userId, password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdatePasswordAsync_ValidUserIdAndPassword_ShouldReturnTrue() {
        // Arrange
        int userId = 1;
        string password = "password";

        _mockRepo.Setup(x => x.User.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdatePasswordAsync(userId, password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdatePasswordAsync_ValidUserIdAndPassword_ShouldHashPasswordCorrectly() {
        // Arrange
        int userId = 1;
        string password = "password";
        string? hashedPassword = null;

        _mockRepo.Setup(x => x.User.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>()))
            .Callback<int, string>((id, hash) => hashedPassword = hash)
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdatePasswordAsync(userId, password);

        // Assert
        result.Should().BeTrue();
        hashedPassword.Should().NotBeNullOrWhiteSpace();
        BCrypt.Net.BCrypt.Verify(password, hashedPassword).Should().BeTrue();
    }

    [Fact]
    public async Task GetUserByEmailAsync_EmailDoesNotExistInTheDatabase_ShouldThrowNotFoundException() {
        // Arrange
        string email = "sam@ple.com";

        _mockRepo.Setup(x => x.User.GetUserByEmailAsync(It.IsAny<string>()))
            .ThrowsAsync(new EmailNotFoundException(email));

        // Act
        Func<Task> act = async() => await _service.GetUserByEmailAsync(email);

        // Assert
        await act.Should().ThrowAsync<EmailNotFoundException>()
            .WithMessage($"The user with email: {email} doesn't exists in the database.");
    }

    [Fact]
    public async Task GetUserByEmailAsync_EmailExistsInTheDatabase_ShouldReturnUserDto() {
        // Arrange
        User user = CreateUser();

        _mockRepo.Setup(x => x.User.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(user.Email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<UserDto>();
    }

    [Fact]
    public async Task GetUserByEmailAsync_ValidEmail_ShouldExecuteRepositoryOnce() {
        // Arrange
        User user = CreateUser();

        _mockRepo.Setup(x => x.User.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(user.Email);

        // Assert
        _mockRepo.Verify(x => x.User.GetUserByEmailAsync(user.Email), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_EmailExistsInTheDatabase_ShouldMapUserToUserDtoCorrectly() {
        // Arrange
        User user = CreateUser();

        _mockRepo.Setup(x => x.User.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(user.Email);

        // Assert
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
        result.DisplayName.Should().Be(user.DisplayName);
        result.UserId.Should().Be(user.UserId);
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

    private static SignUpDto CreateSignUpDto() {
        return new SignUpDto() {
            Username = "test",
            DisplayName = "Test",
            Email = "te@st.com",
            Password = "pass",
            PasswordConfirmation = "pass"
        };
    }

    private static UserForUpdateDto CreateUserForUpdateDto() {
        return new UserForUpdateDto() {
            Username = "newusername",
            DisplayName = "new displayname",
            Address = "new address",
            Email = "new@email.com",
            DisplayPictureUrl = "new picture"
        };
    }

    private static List<User> CreateUsers() {
        return [
            new User() {
                DisplayName = "Test",
                Email = "test@test.com",
                PasswordHash = "$2a$11$wz1mmSBhCfO.AJI4Ll8qZ.KQvqob3mQhN28F7SqB46XLdizFmYYX6",
                Username = "test",
                UserId = 1,
                IsEmailVerified = true,
                DisplayPictureUrl = "currentPictureUrl"
            },
            new User() {
                DisplayName = "Test2",
                Email = "test2@test.com",
                PasswordHash = "$2a$11$wz1mmSBhCfO.AJI4Ll8qZ.KQvqob3mQhN28F7SqB46XLdizFmYYX6",
                Username = "test2",
                UserId = 2,
                IsEmailVerified = true,
                DisplayPictureUrl = "currentPictureUrl"
            }
        ];
    }
}
