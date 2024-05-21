using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Service {
    internal sealed class AuthService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IConfiguration configuration) : IAuthService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IConfiguration _configuration = configuration;

        public async Task<bool> ValidateUser(SignInDto userForAuth) {
            User? user = null;
            string username = userForAuth.Username ?? throw new UsernameNotFoundException(string.Empty);
            string password = userForAuth.Password ?? string.Empty;
            if(IsEmail(username)) {
                user = await _repository.User.GetUserByEmailAsync(username);
            }
            else
                user = await _repository.User.GetUserByUsernameAsync(username);

            bool result = user != null && CheckPassword(password, user.PasswordHash);
            if(!result) {
                _logger.LogWarn($"{nameof(ValidateUser)}: Authentication failed. Wrong username or password.");
            }
            return result;
        }
        public string CreateToken() {
            IConfigurationSection? jwtSetting = _configuration.GetSection("JwtSettings");
            SigningCredentials signingCredentials = GetSigningCredentials(jwtSetting);
            var claims = new List<Claim>();
            JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims, jwtSetting);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private bool CheckPassword(string inputtedPassword, string hashedPassword) => BCrypt.Net.BCrypt.Verify(inputtedPassword, hashedPassword);

        private SigningCredentials GetSigningCredentials(IConfigurationSection jwtSetting) {
            byte[] key = Encoding.UTF8.GetBytes(jwtSetting["secretKey"] ?? "");
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims, IConfigurationSection jwtSetting) {
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSetting["validIssuer"],
                audience: jwtSetting["validAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSetting["expires"])),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }

        private bool IsEmail(string input) {
            string emailPattern = @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$";
            Regex regex = new(emailPattern);
            return regex.IsMatch(input);
        }
    }
}
