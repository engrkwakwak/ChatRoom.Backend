using ChatRoom.Backend.Presentation.ActionFilters;
using Entities.ErrorModel;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class AuthController(IServiceManager service, IConfiguration config) : ControllerBase {
        private readonly IServiceManager _service = service;
        private IConfiguration _config = config;

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
            if (await _service.UserService.HasDuplicateEmail(signUpData.Email!)) {
                throw new ValidationException($"The email {signUpData.Email} is already in used by another user.");
            }

            if (await _service.UserService.HasDuplicateUsername(signUpData.Username!)) {
                throw new ValidationException($"The username {signUpData.Username} is already in used by another user.");
            }
            
            if (signUpData.Password != signUpData.PasswordConfirmation) {
                throw new ValidationException($"The Password and Password Confirmation didnt match.");
            }

            UserDto createdUser = await _service.UserService.InsertUser(signUpData);

            string verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={_service.AuthService.CreateEmailVerificationToken(createdUser)}";
            if (!await _service.EmailService.SendVerificationEmail(createdUser,  verificationLink)) {
                return BadRequest("Something went wrong while sending the email.");
            }

            return Ok(createdUser);
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (String.IsNullOrEmpty(token.TrimEnd()))
            {
                throw new InvalidParameterException("Invalid Request Parameter");
            }

            JwtPayload payload = _service.AuthService.VerifyJwtToken(token);
            if (!await _service.AuthService.VerifyEmail(int.Parse(payload.Sub)))
            {
                throw new Exception("Something went wrong while Verifying the Email.");
            }
            return Redirect($"{_config.GetSection("FrontendUrl").Value}/email-verified");
        }
        
        [HttpGet("is-email-verified")]
        [Authorize]
        public async Task<IActionResult> IsEmailVerified(int id)
        {
            if (String.IsNullOrEmpty(id.ToString()) || id < 1)
            {
                throw new InvalidParameterException("Invalid Request Parameter");
            }

            UserDto user =  await _service.UserService.GetUserById(id);
            return Ok(user.IsEmailVerified);
        }

        [HttpPost("send-email-verification")]
        [Authorize]
        public async Task<IActionResult> SendEmailVerification(int id)
        {
            if(String.IsNullOrEmpty(id.ToString()) || id < 1)
            {
                throw new InvalidParameterException("Invalid Request Parameter");
            }

            UserDto user = await _service.UserService.GetUserById(id);

            if (user.IsEmailVerified)
            {
                return BadRequest("This Email is already verified");
            }

            string verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={_service.AuthService.CreateEmailVerificationToken(user)}";
            if (!await _service.EmailService.SendVerificationEmail(user, verificationLink))
            {
                return BadRequest("Something went wrong while sending the email.");
            };

            return Ok();
        }

    }
}