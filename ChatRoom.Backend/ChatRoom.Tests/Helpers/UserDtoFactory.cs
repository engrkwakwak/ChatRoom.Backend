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

        public static UserForUpdateDto CreateUserForUpdateDto() {
            return new UserForUpdateDto {
                Username = "testusername",
                DisplayName = "Test Display Name",
                Email = "test@email.com"
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

    }
}
