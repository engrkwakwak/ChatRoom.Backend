using ChatRoom.Backend.Presentation.ActionFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Users;
using System.Net.Http.Headers;

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
            return Ok(await _service.UserService.HasDuplicateEmailAsync(email));
        }

        [HttpGet("has-duplicate-username/{username}")]
        public async Task<IActionResult> HasDuplicateUsername(string username) {
            return Ok(await _service.UserService.HasDuplicateUsernameAsync(username));
        }

        [HttpPut("{userId}")]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserForUpdateDto userForUpdate) {
            await _service.UserService.UpdateUserAsync(userId, userForUpdate);

            return NoContent();
        }

        [HttpPost("{userId}/picture"), DisableRequestSizeLimit]
        [Authorize]
        public async Task<IActionResult> UploadDisplayPicture(int userId) {
            var formCollection = await Request.ReadFormAsync();
            var file = formCollection.Files[0];
            string folderName = Path.Combine("Resources", "Images", userId.ToString());
            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if(!Directory.Exists(pathToSave)) 
                Directory.CreateDirectory(pathToSave);
            if (file.Length > 0) {
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
                var fileExtension = Path.GetExtension(fileName);
                fileName = $"{userId}-display-picture{fileExtension}";
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine(folderName, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create)) {
                    file.CopyTo(stream);
                }
                return Ok(dbPath);
            }
            else {
                return BadRequest();
            }
        }
    }
}
