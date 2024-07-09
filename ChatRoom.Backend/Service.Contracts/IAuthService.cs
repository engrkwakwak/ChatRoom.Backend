using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.IdentityModel.Tokens.Jwt;

namespace Service.Contracts {
    public interface IAuthService {
        Task<string?> ValidateUser(SignInDto userForAuth);
        string CreateEmailVerificationToken(UserDto user);
        public JwtPayload VerifyJwtToken(string token);
        public Task<bool> VerifyEmail(int userId);
        int GetUserIdFromJwtToken(string token);
    }
}