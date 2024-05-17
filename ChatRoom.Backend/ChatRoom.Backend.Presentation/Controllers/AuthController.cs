
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
using Shared.DataTransferObjects.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.Backend.Presentation.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IServiceManager service) : ControllerBase
    {
        private readonly IServiceManager _service = service;

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto signUpData )
        {
            // check email has no duplicate
            if (await _service.UserService.HasDuplicateEmail(signUpData.Email))
            {
                throw new ValidationException($"The email {signUpData.Email} is already in used by another user.");
            }

            // check if username has no duplicate
            if (await _service.UserService.HasDuplicateUsername(signUpData.Username))
            {
                throw new ValidationException($"The username {signUpData.Username} is already in used by another user.");
            }

            // check if password and password confirmation matches
            if(signUpData.Password != signUpData.PasswordConfirmation) 
            {
                throw new ValidationException($"The Password and Password Confirmation didnt match.");
            }

            // store user
            UserDto createdUser = await _service.UserService.InsertUser(signUpData);

            // send email later

            return Ok(createdUser);
        }
    }
}
