using Entities.Models;

namespace Contracts {
    public interface IUserRepository {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        public Task<int> HasDuplicateEmail(string email);

        public Task<int> HasDuplicateUsername(string username);

        public Task<User> InsertUser(User user);
    }
}
