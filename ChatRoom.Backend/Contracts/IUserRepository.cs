using Entities.Models;
using Shared.RequestFeatures;

namespace Contracts {
    public interface IUserRepository {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        public Task<int> HasDuplicateEmailAsync(string email);
        public Task<int> HasDuplicateUsernameAsync(string username);
        public Task<User> InsertUser(User user);
        public Task<IEnumerable<User>> SearchUsersByNameAsync(UserParameters userParameters);
    }
}
