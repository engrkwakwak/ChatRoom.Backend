using Shared.DataTransferObjects.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.UnitTest.Helpers
{
    public class AuthDtoFactory
    {
        public static SignInDto CreateValidSignInDto()
        {
            return new SignInDto
            {
                Username = "testuser",
                Password = "password"
            };
        }

        public static SignInDto CreateInvalidSignInDto()
        {
            return new SignInDto
            {
                Username = "",
                Password = ""
            };
        }

        public static SignUpDto CreateValidSignUpDto()
        {
            return new SignUpDto
            {
                Email = "test@email.com",
                DisplayName = "Test User",
                Username = "TestUser",
                Password = "password",
                PasswordConfirmation = "password"
            };
        }

        public static SignUpDto CreateInvalidSignUpDto()
        {
            return new SignUpDto
            {
                Email = "test@email.com",
                DisplayName = "Test User",
                Username = "TestUser",
                Password = "pass", // minlength 8
                PasswordConfirmation = "pass" // minlength 8
            };
        }
    }
}
