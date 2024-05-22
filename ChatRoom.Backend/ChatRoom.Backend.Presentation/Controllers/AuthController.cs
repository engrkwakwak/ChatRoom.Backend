using ChatRoom.Backend.Presentation.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.ComponentModel.DataAnnotations;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IServiceManager service) : ControllerBase {
        private readonly IServiceManager _service = service;

        [HttpPost("signin")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Authenticate([FromBody] SignInDto user) {
            if (!await _service.AuthService.ValidateUser(user))
                return Unauthorized();

            return Ok(new {
                Token = _service.AuthService.CreateToken()
            });
        }

        [HttpPost("signup")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto signUpData) {
            // check email has no duplicate
            if (await _service.UserService.HasDuplicateEmail(signUpData.Email)) {
                throw new ValidationException($"The email {signUpData.Email} is already in used by another user.");
            }

            // check if username has no duplicate
            if (await _service.UserService.HasDuplicateUsername(signUpData.Username)) {
                throw new ValidationException($"The username {signUpData.Username} is already in used by another user.");
            }

            // check if password and password confirmation matches
            if (signUpData.Password != signUpData.PasswordConfirmation) {
                throw new ValidationException($"The Password and Password Confirmation didnt match.");
            }

            // store user
            UserDto createdUser = await _service.UserService.InsertUser(signUpData);

            // send email later

            return Ok(createdUser);
        }
    }
}