using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace Service {
    internal sealed class UserService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IUserService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<UserDto> GetUserByIdAsync(int userId) {
            User user = await GetUserAndCheckIfItExists(userId);

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

        public async Task<UserDto> GetUserById(int id)
        {
            User? user = await _repository.User.GetUserByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }


        public async Task UpdateUserAsync(int userId, UserForUpdateDto userForUpdate) {
            User user = await GetUserAndCheckIfItExists(userId);

            if (user.Email != userForUpdate.Email)
                user.IsEmailVerified = false;
            _mapper.Map(userForUpdate, user);         
        
            int rowsAffected = await _repository.User.UpdateUserAsync(user);

            if (rowsAffected <= 0) {
                _logger.LogWarn($"Failed to update the user with id {user.UserId}. Total rows affected: {rowsAffected}. At {nameof(UserService)} - {nameof(UpdateUserAsync)}.");
                throw new UserUpdateFailedException(user.UserId);
            }
        }

        private async Task<User> GetUserAndCheckIfItExists(int userId) {
            User? user = await _repository.User.GetUserByIdAsync(userId);
            return user is null ? throw new UserIdNotFoundException(userId) : user;
        }

        public async Task<IEnumerable<UserDto>> SearchUsersByNameAsync(UserParameters userParameter)
        {
            IEnumerable<User> users = await _repository.User.SearchUsersByNameAsync(userParameter);
            IEnumerable<UserDto> userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return userDtos;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            int affectedRows = await _repository.User.UpdatePasswordAsync(userId, passwordHash);   

            return (affectedRows > 0);
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            User? user = await _repository.User.GetUserByEmailAsync(email) ?? throw new UsernameNotFoundException(email);
            return _mapper.Map<UserDto>(user);
        }
    }
}
