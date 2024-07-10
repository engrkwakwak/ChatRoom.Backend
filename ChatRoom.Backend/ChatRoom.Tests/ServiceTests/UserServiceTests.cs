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
        int userId = 1;
        User user = CreateUser();
        UserForUpdateDto userForUpdate = CreateUserForUpdateDto();
        user.DisplayPictureUrl = null;
        userForUpdate.DisplayPictureUrl = null;

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
    public async Task UpdateUserAsync_RepositoryUpdateFails_ShouldThrowUserUpdateFailedException() {

    }

    [Fact]
    public async Task UpdateUserAsync_RepositoryUpdateFails_ShouldLogWarning() {

    }

    [Fact]
    public async Task UpdateUserAsync_ValidUserAndUpdateData_ShouldUpdateUserSuccessfully() {

    }

    [Fact]
    public async Task UpdateUserAsync_ValidUserAndUpdateData_ShouldRemoveUserFromCache() {

    }

    [Fact]
    public async Task GetUsersAsync_RepositoryReturnsEmptyList_ShouldReturnEmptyUsersAndMetadata() {

    }

    [Fact]
    public async Task GetUsersAsync_ValidUserParameters_ShouldReturnUsersAndMetaData() {
        // should call repo once
        // map properly
    }

    [Fact]
    public async Task GetUsersAsync_ValidUserParameters_ShouldMapUsersToUserDtosCorrectly() {

    }

    [Fact]
    public async Task UpdatePasswordAsync_RepositoryUpdateFails_ShouldReturnFalse() {

    }

    [Fact]
    public async Task UpdatePasswordAsync_ValidUserIdAndPassword_ShouldReturnTrue() {

    }

    [Fact]
    public async Task UpdatePasswordAsync_ValidUserIdAndPassword_ShouldHashPasswordCorrectly() {

    }

    [Fact]
    public async Task GetUserByEmailAsync_EmailDoesNotExistInTheDatabase_ShouldThrowNotFoundException() {

    }

    [Fact]
    public async Task GetUserByEmailAsync_EmailExistsInTheDatabase_ShouldReturnUserDto() {

    }

    [Fact]
    public async Task GetUserByEmailAsync_ValidEmail_ShouldExecuteRepositoryOnce() {

    }

    [Fact]
    public async Task GetUserByEmailAsync_ValidEmail_ShouldMapUserToUserDtoCorrectly() {

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
}
