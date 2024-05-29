using Entities.Models;
using Shared.RequestFeatures;
using System.Threading.Tasks;

namespace Contracts {
    public interface IUserRepository {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User> InsertUserAsync(User user);
        Task<int> UpdateUserAsync(User user);
        Task<int> VerifyEmailAsync(int id);
        public Task<int> HasDuplicateEmailAsync(string email);
        public Task<int> HasDuplicateUsernameAsync(string username);
        public Task<IEnumerable<User>> SearchUsersByNameAsync(UserParameters userParameters);
    }
}
