using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
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

        public string CreateEmailVerificationToken(UserDto userDto)
        {
            User? _user = _mapper.Map<User>(userDto);
            string token = CreateToken(_user);
            return token;
        }

        public async Task<string?> ValidateUser(SignInDto userForAuth) {
            User? user;
            if (IsEmail(userForAuth.Username!))
                user = await _repository.User.GetUserByEmailAsync(userForAuth.Username!);
            else
                user = await _repository.User.GetUserByUsernameAsync(userForAuth.Username!);

            if (user == null || !CheckPassword(userForAuth.Password!, user.PasswordHash))
            {
                _logger.LogWarn($"{nameof(ValidateUser)}: Authentication failed. Wrong username or password.");
                return null;
            }
            string token = CreateToken(user!);
            return token;
        }

        public JwtPayload VerifyJwtToken(string token)
        {
            if (token.IsNullOrEmpty())
            {
                throw new InvalidParameterException("Invalid Token,the token cannot be empty.");
            }

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

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
            if (token.IsNullOrEmpty())
            {
                throw new InvalidParameterException("Invalid Token,the token cannot be empty.");
            }

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

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
        private IConfigurationSection GetJwtSettingFromConfiguration()
        {
            IConfigurationSection jwtSetting = _configuration.GetSection("JwtSettings");
            if (!jwtSetting.Exists())
                throw new Exception("Something went wrong while processing the request. Unable generate authentication token.");
            return jwtSetting;
        }

        private string CreateToken(User user)
        {
            IConfigurationSection? jwtSetting = GetJwtSettingFromConfiguration();
            SigningCredentials signingCredentials = GetSigningCredentials();
            List<Claim> claims = GetClaims(user);
            JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims, jwtSetting);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private static bool CheckPassword(string inputtedPassword, string hashedPassword) => BCrypt.Net.BCrypt.Verify(inputtedPassword, hashedPassword);

        private SigningCredentials GetSigningCredentials() {
            byte[] key = Encoding.UTF8.GetBytes(_configuration["TOKEN_SECRET_KEY"] ?? throw new JwtSecretKeyNotFoundException());
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
        private List<Claim> GetClaims(User user) => [
                new(JwtRegisteredClaimNames.Sub, user!.UserId.ToString()),
                new("display-name", user.DisplayName),
                new("display-picture", user.DisplayPictureUrl ?? ""),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString())
            ];


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
