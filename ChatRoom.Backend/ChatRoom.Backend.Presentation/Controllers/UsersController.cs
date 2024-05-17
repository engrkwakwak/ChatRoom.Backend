using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/users")]
    [ApiController]
    public class UsersController(IServiceManager service) : ControllerBase {
        private readonly IServiceManager _service = service;

        [HttpGet("has-duplicate-email")]
        public async Task<IActionResult> HasDuplicateEmail(string email)
        {
            return Ok(await _service.UserService.HasDuplicateEmail(email));
        }

        [HttpGet("has-duplicate-username")]
        public async Task<IActionResult> HasDuplicateUsername(string username)
        {
            return Ok(await _service.UserService.HasDuplicateUsername(username));
        }
    }
}
