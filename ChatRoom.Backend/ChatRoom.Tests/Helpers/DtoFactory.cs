using Shared.DataTransferObjects.Users;

namespace ChatRoom.UnitTest.Helpers {
    public static class DtoFactory {
        public static UserDto CreateUserDto() {
            return new UserDto {
                DisplayName = "Test Display Name",
                Email = "test@test.com",
                Username = "testusername",
                UserId = 500,
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
    }
}
