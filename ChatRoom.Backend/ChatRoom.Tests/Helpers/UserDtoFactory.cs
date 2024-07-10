using Entities.Exceptions;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;

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
    }
}
