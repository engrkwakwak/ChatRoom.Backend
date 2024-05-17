namespace Service.Contracts {
    public interface IUserService {
        public Task<bool> HasDuplicateEmail(string email);
    }
}
