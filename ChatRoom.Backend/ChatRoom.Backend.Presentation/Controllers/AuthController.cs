using ChatRoom.Backend.Presentation.ActionFilters;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Razor.Templating.Core;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Email;
using Shared.DataTransferObjects.Users;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IServiceManager service) : ControllerBase {
        private readonly IServiceManager _service = service;

        [HttpPost("signin")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Authenticate([FromBody] SignInDto user)
        {
            if (!await _service.AuthService.ValidateUser(user))
                return Unauthorized();

            return Ok(new
            {
                Token = _service.AuthService.CreateToken()
            });
        }

        [HttpPost("signup")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto signUpData) {
            // check email has no duplicate
            if (await _service.UserService.HasDuplicateEmail(signUpData.Email!)) {
                throw new ValidationException($"The email {signUpData.Email} is already in used by another user.");
            }

            // check if username has no duplicate
            if (await _service.UserService.HasDuplicateUsername(signUpData.Username!)) {
                throw new ValidationException($"The username {signUpData.Username} is already in used by another user.");
            }

            // check if password and password confirmation matches
            if (signUpData.Password != signUpData.PasswordConfirmation) {
                throw new ValidationException($"The Password and Password Confirmation didnt match.");
            }

            // store user
            UserDto createdUser = await _service.UserService.InsertUser(signUpData);

            // send email
            string verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={_service.AuthService.CreateEmailVerificationToken(createdUser)}";
            if (!await _service.EmailService.SendVerificationEmail(createdUser,  verificationLink))
            {
                return BadRequest("Something went wrong while sending the email.");
            };

            return Ok(createdUser);
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            // check for if token is correct
            JwtPayload payload = _service.AuthService.VerifyJwtToken(token);

            // verify

            return Ok();

        }

        [HttpGet("is-email-verified")]
        public async Task<IActionResult> IsEmailVerified(int id)
        {
            UserDto user =  await _service.UserService.GetUserById(id);

            return Ok(user.IsEmailVerified);
        }

        [HttpPost("send-email-verification")]
        [Authorize]
        public async Task<IActionResult> SendEmailVerification(int id)
        {
            
            UserDto user =  await _service.UserService.GetUserById(id);

            if (user.IsEmailVerified)
            {
                return BadRequest("This Email is already verified");
            }

            string verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={_service.AuthService.CreateEmailVerificationToken(user)}";   
            if (!await _service.EmailService.SendVerificationEmail(user,  verificationLink))
            {
                return BadRequest("Something went wrong while sending the email.");
            };

            return Ok();
        }

    }
}