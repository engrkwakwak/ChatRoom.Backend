using ChatRoom.Backend.Presentation.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Auth;

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
    }
}