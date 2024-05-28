using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;

namespace Service.Contracts {
    public interface IUserService {
        Task<UserDto> GetUserByIdAsync(int userId);

        Task<bool> HasDuplicateEmail(string email);
        Task<bool> HasDuplicateUsername(string username);

        Task<UserDto> InsertUser(SignUpDto signUp);

        Task UpdateUserAsync(int userId, UserForUpdateDto userForUpdate);
    }
}
