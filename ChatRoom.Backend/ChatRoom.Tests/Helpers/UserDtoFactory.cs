using Entities.Exceptions;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System;
using Entities.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace ChatRoom.UnitTest.Helpers {
    public static class UserDtoFactory {
        public static UserDto CreateUserDto(int userId = 500) {
            return new UserDto {
                DisplayName = "Test Display Name",
                Email = "test@test.com",
                Username = "testusername",
                UserId = userId,
            };
        }        

        public static UserForUpdateDto CreateUserForUpdateDto()
        {
            return new UserForUpdateDto
            {
                Username = "testusername",
                DisplayName = "Test Display Name",
                Email = "test@email.com"
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithEmptyUsername()
        {
            return new UserForUpdateDto
            {
                DisplayName = "Test Display Name",
                Email = "test@test.com",
                Username = ""
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithUsernameGreaterThan20Characters()
        {
            return new UserForUpdateDto
            {
                DisplayName = "Test Display Name",
                Email = "test@test.com",
                Username = "123456789012345678901"
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithEmptyDisplayName()
        {
            return new UserForUpdateDto
            {
                DisplayName = "",
                Email = "test@test.com",
                Username = "testusername"
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithDisplayNameGreaterThan50Characters()
        {
            return new UserForUpdateDto
            {
                DisplayName = "123456789012345678901234567890123456789012345678901",
                Email = "test@test.com",
                Username = "testusername"
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithEmptyEmail()
        {
            return new UserForUpdateDto
            {
                DisplayName = "Test Display Name",
                Email = "",
                Username = "testusername"
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithEmailGreaterThan100Characters()
        {
            return new UserForUpdateDto
            {
                DisplayName = "Test Display Name",
                Email = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901",
                Username = "testusername"
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithInvalidEmailFormat()
        {
            return new UserForUpdateDto
            {
                DisplayName = "Test Display Name",
                Email = "1234567890123456789012345678901234",
                Username = "testusername"
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithAddressGreaterThan100Characters()
        {
            return new UserForUpdateDto
            {
                DisplayName = "Test Display Name",
                Email = "test@email.com",
                Username = "testusername",
                Address = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901"
            };
        }

        public static UserForUpdateDto CreateUserForUpdateDtoWithDisplayPictureUrlGreaterThan200Characters()
        {
            return new UserForUpdateDto
            {
                DisplayName = "Test Display Name",
                Email = "test@email.com",
                Username = "testusername",
                DisplayPictureUrl = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890112345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901"
            };
}
        public static UserForUpdateDto CreateInvalidUserForUpdateDto() {
            return new UserForUpdateDto();
        }

        public static IEnumerable<UserDto> CreateListOfUserDto()
        {
            return [
                new UserDto() {
                    DisplayName = "Test User 1",
                    Email = "test@user.1",
                    Username = "user1"
                },
                new UserDto() {
                    DisplayName = "Test User 2",
                    Email = "test@user.2",
                    Username = "user2"
                },
                new UserDto() {
                    DisplayName = "Test User 3",
                    Email = "test@user.3",
                    Username = "user3"
                },
            ];
        }

        public static IEnumerable<UserDto> CreateEmptyListOfUserDto()
        {
            return [];
        }

        public static User CreateUser(
            int userId = 1,
            string displayName = "Test User",
            string email = "test@email.com",
            string passwordHash = "$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa", // decrypt: password
            string username = "testusername"
        )
        {
            return new()
            {
                UserId = userId,
                DisplayName = displayName,
                Email = email,
                PasswordHash = passwordHash,
                Username = username,
            };
        }

        public static string GenerateExpiredJwtToken(string tokenSecretKey = "cHaTrOoM-sEcReTkEy-1$2$3$4$5$6$7$8$9$0$-GREATERTHAN256BYTES")
        {
            User user = CreateUser();
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

        public static string GenerateJwtToken(string tokenSecretKey = "cHaTrOoM-sEcReTkEy-1$2$3$4$5$6$7$8$9$0$-GREATERTHAN256BYTES", User? user=null)
        {
            User _user = user ?? CreateUser();
            List<Claim> claims = [
                new(JwtRegisteredClaimNames.Sub, _user!.UserId.ToString()),
                new("display-name", _user.DisplayName),
                new("display-picture", _user.DisplayPictureUrl ?? ""),
                new(ClaimTypes.NameIdentifier, _user.UserId.ToString())
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

        public static string GenerateJwtTokenWithoutSubClaim(string tokenSecretKey = "cHaTrOoM-sEcReTkEy-1$2$3$4$5$6$7$8$9$0$-GREATERTHAN256BYTES")
        {
            User user = CreateUser();
            List<Claim> claims = [
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

        public static UserDisplayDto CreateUserDisplayDto(int userId=1, string displayName="Test Name", string displayPictureUrl="test-url")
        {
            return new()
            {
                DisplayPictureUrl = displayPictureUrl,
                DisplayName = displayName,
                UserId = userId
            };
        }
    }
}
