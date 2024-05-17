using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.Security.Cryptography;
using System.Text;

namespace Service {
    internal sealed class UserService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IUserService
    {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> HasDuplicateEmail(string email)
        {
            if(await _repository.User.HasDuplicateEmail(email) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> HasDuplicateUsername(string username)
        {
            if (await _repository.User.HasDuplicateUsername(username) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<UserDto> InsertUser(SignUpDto userSignUpData)
        {
            User userToInsert = _mapper.Map<User>(userSignUpData);

            userToInsert.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userSignUpData.Password);

            User createdUser = await _repository.User.InsertUser(userToInsert);

            UserDto userToReturn = _mapper.Map<UserDto>(createdUser);

            return userToReturn;
        }

    }
}
