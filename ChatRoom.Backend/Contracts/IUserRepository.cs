using System.Data.SqlTypes;

namespace Contracts {
    public interface IUserRepository {
        public Task<int> HasDuplicateEmail(string email);

        public Task<int> HasDuplicateUsername(string username);
    }
}
