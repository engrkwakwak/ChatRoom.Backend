using Entities.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.UnitTest.Helpers
{
    public class UserDtoFactory
    {
        public static User CreateUser(
            string displayName="Test User", 
            string email="test@email.com", 
            string passwordHash= "$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa", // decrypt: password
            string username="testusername"
        )
        {
            return new()
            {
                DisplayName = displayName,
                Email = email,
                PasswordHash = passwordHash,
                Username = username,
            };
        }

        public static string GenerateExpiredJwtToken(string tokenSecretKey="cHaTrOoM-sEcReTkEy-1$2$3$4$5$6$7$8$9$0$-GREATERTHAN256BYTES")
        {
            User user  = CreateUser();
            List<Claim> claims = [
                new(JwtRegisteredClaimNames.Sub, user!.UserId.ToString()),
                new("display-name", user.DisplayName),
                new("display-picture", user.DisplayPictureUrl ?? ""),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString())
            ];
            byte[] key = Encoding.UTF8.GetBytes(tokenSecretKey);
            SigningCredentials signingCredentials = new(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            ); 
            JwtSecurityToken tokenOptions = new(
                issuer: "validIssuer",
                audience: "validAudience",
                claims: claims,
                expires: DateTime.Now.Subtract(TimeSpan.FromMinutes(60)),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public static string GenerateJwtToken(string tokenSecretKey="cHaTrOoM-sEcReTkEy-1$2$3$4$5$6$7$8$9$0$-GREATERTHAN256BYTES")
        {
            User user  = CreateUser();
            List<Claim> claims = [
                new(JwtRegisteredClaimNames.Sub, user!.UserId.ToString()),
                new("display-name", user.DisplayName),
                new("display-picture", user.DisplayPictureUrl ?? ""),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString())
            ];
            byte[] key = Encoding.UTF8.GetBytes(tokenSecretKey);
            SigningCredentials signingCredentials = new(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            ); 
            JwtSecurityToken tokenOptions = new(
                issuer: "validIssuer",
                audience: "validAudience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
    }
}
