using Entities.Models;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;

namespace Service.Contracts {
    public interface IUserService {
        public Task<bool> HasDuplicateEmail(string email);

        public Task<bool> HasDuplicateUsername(string username);

        public Task<UserDto> InsertUser(SignUpDto signUp);

        public Task<UserDto> GetUserById(int id);
    }
}
