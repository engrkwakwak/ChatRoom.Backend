using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/users")]
    [ApiController]
    public class UsersController(IServiceManager service) : ControllerBase {
        private readonly IServiceManager _service = service;

        [HttpGet]
        [Authorize]
        public IActionResult GetUser() {
            return Ok("User is here.");
        }
    }
}
