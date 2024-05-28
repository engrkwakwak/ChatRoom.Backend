using Entities.Models;
using System.Threading.Tasks;

namespace Contracts {
    public interface IUserRepository {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);

        Task<int> HasDuplicateEmailAsync(string email);
        Task<int> HasDuplicateUsernameAsync(string username);

        Task<User> InsertUserAsync(User user);

        Task<int> UpdateUserAsync(User user);

        Task<int> VerifyEmailAsync(int id);
    }
}
