using ChatRoom.Backend.Presentation.ActionFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Users;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/users")]
    [ApiController]
    public class UsersController(IServiceManager service) : ControllerBase {
        private readonly IServiceManager _service = service;

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int userId) {
            UserDto user = await _service.UserService.GetUserByIdAsync(userId);
            return Ok(user);
        }

        [HttpGet("has-duplicate-email/{email}")]
        public async Task<IActionResult> HasDuplicateEmail(string email) {
            return Ok(await _service.UserService.HasDuplicateEmail(email));
        }

        [HttpGet("has-duplicate-username/{username}")]
        public async Task<IActionResult> HasDuplicateUsername(string username) {
            return Ok(await _service.UserService.HasDuplicateUsername(username));
        }

        [HttpPut("{userId}")]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserForUpdateDto userForUpdate) {
            await _service.UserService.UpdateUserAsync(userId, userForUpdate);

            return NoContent();
        }
    }
}
