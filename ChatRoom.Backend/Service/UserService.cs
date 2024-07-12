using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Components.Web;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace Service; 
public sealed class UserService(
    IRepositoryManager repository,
    ILoggerManager logger,
    IMapper mapper,
    IRedisCacheManager cache,
    IFileManager fileManager
) : IUserService {

    private readonly IRepositoryManager _repository = repository;
    private readonly ILoggerManager _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IRedisCacheManager _cache = cache;
    private readonly IFileManager _fileManager = fileManager;

    public async Task<UserDto> GetUserByIdAsync(int userId) {
        string cacheKey = $"user:{userId}";
        User user = await _cache.GetCachedDataAsync<User>(cacheKey);
        if(user != null)
        {
            return _mapper.Map<UserDto>(user);
        }

        user = await GetUserAndCheckIfItExists(userId);
        await _cache.SetCachedDataAsync<User>(cacheKey, user, TimeSpan.FromMinutes(30));
        UserDto userDto = _mapper.Map<UserDto>(user);
        return userDto;
    }

    public async Task<bool> HasDuplicateEmailAsync(string email) {
        return await _repository.User.HasDuplicateEmailAsync(email) > 0;
    }

    public async Task<bool> HasDuplicateUsernameAsync(string username) {
        return await _repository.User.HasDuplicateUsernameAsync(username) > 0;
    }

    public async Task<UserDto> InsertUserAsync(SignUpDto userSignUpData) {
        User userEntity = _mapper.Map<User>(userSignUpData);
        userEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userSignUpData.Password);

        userEntity = await _repository.User.InsertUserAsync(userEntity);

        UserDto userToReturn = _mapper.Map<UserDto>(userEntity);

        return userToReturn;
    }

    public async Task UpdateUserAsync(int userId, UserForUpdateDto userForUpdate) {
        User user = await GetUserAndCheckIfItExists(userId);

        if (ShouldInvalidateEmail(user.Email, userForUpdate.Email!))
            user.IsEmailVerified = false;

        if (ShouldDeleteDisplayPicture(user.DisplayPictureUrl, userForUpdate.DisplayPictureUrl)) {
            await _fileManager.DeleteImageAsync(user.DisplayPictureUrl!);
        }

        _mapper.Map(userForUpdate, user);         
    
        int rowsAffected = await _repository.User.UpdateUserAsync(user);

        if (rowsAffected <= 0) {
            _logger.LogWarn($"Failed to update the user with id {user.UserId}. " +
                $"Total rows affected: {rowsAffected}. At {nameof(UserService)} - {nameof(UpdateUserAsync)}.");
            throw new UserUpdateFailedException(user.UserId);
        }

        await _cache.RemoveDataAsync(key: $"user:{userId}");
    }

    public async Task<(IEnumerable<UserDto> users, MetaData? metaData)> GetUsersAsync(UserParameters userParameters) {
        PagedList<User> usersWithMetaData = await _repository.User.GetUsersAsync(userParameters);

        IEnumerable<UserDto> usersDto = _mapper.Map<IEnumerable<UserDto>>(usersWithMetaData);
        return (users: usersDto, metaData: usersWithMetaData.MetaData);
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string password)
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        int affectedRows = await _repository.User.UpdatePasswordAsync(userId, passwordHash);   

        return (affectedRows > 0);
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        User user = await _repository.User.GetUserByEmailAsync(email) 
            ?? throw new EmailNotFoundException(email);
        return _mapper.Map<UserDto>(user);
    }

    private async Task<User> GetUserAndCheckIfItExists(int userId) {
        User user = await _repository.User.GetUserByIdAsync(userId)
            ?? throw new UserIdNotFoundException(userId);
        return user;
    }

    private static bool ShouldInvalidateEmail(string currentEmail, string newEmail) {
        return !string.Equals(currentEmail, newEmail, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldDeleteDisplayPicture(string? currentPictureUrl, string? newPictureUrl) {
        bool hasCurrentPicture = !string.IsNullOrEmpty(currentPictureUrl);
        bool isPictureChanged = !string.Equals(currentPictureUrl, newPictureUrl, StringComparison.OrdinalIgnoreCase);

        return isPictureChanged && hasCurrentPicture;
    }
}
