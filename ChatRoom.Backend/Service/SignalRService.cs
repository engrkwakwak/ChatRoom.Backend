using Contracts;
using Service.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Service {
    internal sealed class SignalRService(ILoggerManager logger) : ISignalRService {
        private readonly ILoggerManager _logger = logger;

        public int GetUserIdFromToken(string token) {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken is null) {
                _logger.LogError("Failed to fetch the user id from the token provided. Token is null.");
                return 0;
            }

            string userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            return int.Parse(userId);
        }
    }
}
