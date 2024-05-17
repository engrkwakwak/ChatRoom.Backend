
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;
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
        public async Task<IActionResult> SignUp([FromBody] SignUpDto param )
        {
            // check for email duplication
            if (await _service.UserService.HasDuplicateEmail(param.Email))
            {
                throw new ValidationException("The Email is already in used.");
            }

            // check for username duplication

            // store user and check if stored

            // send email

            return Ok("test");
        }
    }
}
