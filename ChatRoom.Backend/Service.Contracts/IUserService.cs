using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace Service.Contracts {
    public interface IUserService {
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<UserDto> InsertUserAsync(SignUpDto signUp);

        Task<bool> HasDuplicateEmailAsync(string email);
        Task<bool> HasDuplicateUsernameAsync(string username);

        Task UpdateUserAsync(int userId, UserForUpdateDto userForUpdate);

        Task<IEnumerable<UserDto>> SearchUsersByNameAsync(UserParameters userParameter);
        Task<(IEnumerable<UserDto> users, MetaData? metaData)> GetUsersAsync(UserParameters userParameters);
    }
}
