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

        ///<summary>
        ///<para>Gets all users based on the given list of ids</para>
        ///</summary>
        ///<param name="ids">The converted list of ids to be pass onto the stored procedure. Should be in example format "1,2,3,4,5". <para><seealso href="https://stackoverflow.com/questions/14959824/convert-a-list-into-a-comma-separated-string" /></para></param>
        Task<IEnumerable<User>> GetUsersByIdsAsync(string ids);
        Task<PagedList<User>> GetUsersAsync(UserParameters userParameters);

        Task<int> UpdatePasswordAsync(int userId, string passwordHash);
    }
}
