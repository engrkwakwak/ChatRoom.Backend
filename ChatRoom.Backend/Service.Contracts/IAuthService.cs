using Shared.DataTransferObjects.Auth;

namespace Service.Contracts {
    public interface IAuthService {
        Task<bool> ValidateUser(SignInDto userForAuth);
        string CreateToken();
    }
}
