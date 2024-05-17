namespace Contracts {
    public interface IUserRepository {
        public Task<int> HasDuplicateEmail(string email);
    }
}
