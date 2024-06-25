using AutoMapper;
using Contracts;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Service {
    internal sealed class AuthService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IConfiguration configuration, IRedisCacheManager cache) : IAuthService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IConfiguration _configuration = configuration;
        private readonly IRedisCacheManager _cache = cache;
        private User? User { get; set; }

        public string CreateToken()
        {
            IConfigurationSection? jwtSetting = _configuration.GetSection("JwtSettings");
            SigningCredentials signingCredentials = GetSigningCredentials(jwtSetting);
            var claims = GetClaims();
            JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims, jwtSetting);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public string CreateEmailVerificationToken(UserDto user)
        {
            IConfigurationSection? jwtSetting = _configuration.GetSection("EmailJwtSettings");
            SigningCredentials signingCredentials = GetSigningCredentials(jwtSetting);
            User = _mapper.Map<User>(user);
            var claims = GetClaims();
            JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims, jwtSetting);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public async Task<bool> ValidateUser(SignInDto userForAuth) {
            if (IsEmail(userForAuth.Username!))
                User = await _repository.User.GetUserByEmailAsync(userForAuth.Username!);
            else
                User = await _repository.User.GetUserByUsernameAsync(userForAuth.Username!);

            bool result = User != null && CheckPassword(userForAuth.Password!, User.PasswordHash);
            if (!result) {
                _logger.LogWarn($"{nameof(ValidateUser)}: Authentication failed. Wrong username or password.");
            }

            return (result);
        }
        public JwtPayload VerifyJwtToken(string token)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            JwtSecurityToken securityToken = jwtSecurityTokenHandler.ReadJwtToken(token);
            
            if(IsSecurityTokenExpired(securityToken))
            {
                _logger.LogError($"{nameof(VerifyJwtToken)}: Verification Failed. The token has expired.");
                throw new Exception("Verification Failed. The token has expired.");
            }
            return securityToken.Payload;
        }
        public async Task<bool> VerifyEmail(int userId)
        {
            int affectedRows = await _repository.User.VerifyEmailAsync(userId);
            await _cache.RemoveDataAsync($"user:{userId}");
            return affectedRows > 0;
        }

        public int GetUserIdFromJwtToken(string token)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            JwtSecurityToken securityToken = jwtSecurityTokenHandler.ReadJwtToken(token);
            if(securityToken.Subject == null)
            {
                throw new Exception("Invalid token");
            }
            return int.Parse(securityToken.Subject);
        }

        /*
            Private Methods 
        */
        private static bool CheckPassword(string inputtedPassword, string hashedPassword) => BCrypt.Net.BCrypt.Verify(inputtedPassword, hashedPassword);

        private static SigningCredentials GetSigningCredentials(IConfigurationSection jwtSetting) {
            byte[] key = Encoding.UTF8.GetBytes(jwtSetting["secretKey"] ?? "");
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private static JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims, IConfigurationSection jwtSetting) {
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSetting["validIssuer"],
                audience: jwtSetting["validAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSetting["expires"])),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }
        private List<Claim> GetClaims() {
            return new List<Claim> {
                new(JwtRegisteredClaimNames.Sub, User!.UserId.ToString()),
                new("display-name", User.DisplayName),
                new("display-picture", User.DisplayPictureUrl ?? ""),
                new(ClaimTypes.NameIdentifier, User.UserId.ToString())
            };
        }


        private static bool IsSecurityTokenExpired(JwtSecurityToken token) {
            return (DateTime.Compare(DateTime.UtcNow, token.Payload.ValidTo.ToUniversalTime()) > 0);
        }

        private static bool IsEmail(string input) {
            string emailPattern = @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$";
            Regex regex = new(emailPattern);
            return regex.IsMatch(input);
        }
    }
}
