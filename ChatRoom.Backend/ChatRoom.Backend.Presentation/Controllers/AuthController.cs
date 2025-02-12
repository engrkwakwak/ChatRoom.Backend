﻿using ChatRoom.Backend.Presentation.ActionFilters;
using Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IServiceManager service, IConfiguration config) : ControllerBase {
        private readonly IServiceManager _service = service;
        private readonly IConfiguration _config = config;

        [HttpPost("signin")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Authenticate([FromBody] SignInDto user)
        {
            string? token = await _service.AuthService.ValidateUser(user);
            if (token.IsNullOrEmpty())
                return Unauthorized();

            return Ok(new TokenDto
            {
                Token = token
            });
        }

        [HttpPost("signup")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto signUpData) {
            if (await _service.UserService.HasDuplicateEmailAsync(signUpData.Email!)) {
                throw new ValidationException($"The email {signUpData.Email} is already in used by another user.");
            }

            if (await _service.UserService.HasDuplicateUsernameAsync(signUpData.Username!)) {
                throw new ValidationException($"The username {signUpData.Username} is already in used by another user.");
            }
            
            if (signUpData.Password != signUpData.PasswordConfirmation) {
                throw new ValidationException($"The Password and Password Confirmation didnt match.");
            }

            UserDto createdUser = await _service.UserService.InsertUserAsync(signUpData);

            string token = _service.AuthService.CreateEmailVerificationToken(createdUser);
            string verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";
            if (!await _service.EmailService.SendVerificationEmail(createdUser,  verificationLink, token)) {
                throw new EmailNotSentException("Something went wrong while sending the verification email.");
            }

            return Ok(createdUser);
        }

        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto)
        {   
            if(updatePasswordDto.Password != updatePasswordDto.PasswordConfirmation)
            {
                throw new ValidationException($"The passwords doesnt match.");
            }

            if (await _service.EmailService.IsEmailTokenUsed(updatePasswordDto.Token))
            {
                throw new InvalidParameterException("Invalid Link. This link has expired or already been used.");
            }

            _service.AuthService.VerifyJwtToken(updatePasswordDto.Token);
            int userId = _service.AuthService.GetUserIdFromJwtToken(updatePasswordDto.Token);

            if(!await _service.UserService.UpdatePasswordAsync(userId, updatePasswordDto.Password))
                throw new UserUpdateFailedException(userId);

            await _service.EmailService.RemoveTokenFromCache(updatePasswordDto.Token);

            return NoContent();
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (token.IsNullOrEmpty())
            {
                throw new InvalidParameterException("Invalid Request Parameter");
            }

            if (await _service.EmailService.IsEmailTokenUsed(token))
            {
                throw new InvalidParameterException("Invalid Link. This link has expired or already been used.");
            }

            JwtPayload payload = _service.AuthService.VerifyJwtToken(token);
            if (!await _service.AuthService.VerifyEmail(int.Parse(payload.Sub)))
            {
                throw new EmailVerificationFailedException("Something went wrong while Verifying the Email.");
            }

            await _service.EmailService.RemoveTokenFromCache(token);

            return Redirect($"{_config.GetSection("FrontendUrl").Value}/#/email-verified");
        }
        
        [HttpGet("is-email-verified")]
        [Authorize]
        public async Task<IActionResult> IsEmailVerified(int id)
        {
            if (id < 1)
            {
                throw new InvalidParameterException("Invalid Request Parameter");
            }

            UserDto user =  await _service.UserService.GetUserByIdAsync(id);
            return Ok(user.IsEmailVerified);
        }

        [HttpPost("send-email-verification")]
        [Authorize]
        public async Task<IActionResult> SendEmailVerification(int id)
        {
            if(id < 1)
            {
                throw new InvalidParameterException("Invalid Request Parameter");
            }

            UserDto user = await _service.UserService.GetUserByIdAsync(id);

            if (user.IsEmailVerified)
            {
                return BadRequest("This Email is already verified");
            }

            string token = _service.AuthService.CreateEmailVerificationToken(user);
            string verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";
            if (!await _service.EmailService.SendVerificationEmail(user, verificationLink, token))
            {
                return BadRequest("Something went wrong while sending the email.");
            };

            return NoContent();
        }

        [HttpPost("send-password-reset-link")]
        public async Task<IActionResult> SendPasswordResetLink(int userId)
        {
            if(userId < 1)
            {
                throw new InvalidParameterException("Invalid Request Parameter");
            }

            UserDto user = await _service.UserService.GetUserByIdAsync(userId);

            string token = _service.AuthService.CreateEmailVerificationToken(user);
            string passwordResetLink = $"{_config.GetSection("FrontendUrl").Get<string>()}/#/auth/reset-password?token={token}";
            if (!await _service.EmailService.SendPasswordResetLink(user, passwordResetLink, token))
            {
                return BadRequest("Something went wrong while sending the email.");
            };
            return NoContent();
        }

        [HttpPost("send-password-reset-link-via-email")]
        public async Task<IActionResult> SendPasswordResetLinkByEmail(string email)
        {
            if (email.IsNullOrEmpty())
            {
                throw new InvalidParameterException("Invalid Request Parameter");
            }

            UserDto user = await _service.UserService.GetUserByEmailAsync(email);

            string token = _service.AuthService.CreateEmailVerificationToken(user);
            string passwordResetLink = $"{_config.GetSection("FrontendUrl").Get<string>()}/#/auth/reset-password?token={token}";
            if (!await _service.EmailService.SendPasswordResetLink(user, passwordResetLink, token))
            {
                return BadRequest("Something went wrong while sending the email.");
            };
            return NoContent();
        }

    }
}