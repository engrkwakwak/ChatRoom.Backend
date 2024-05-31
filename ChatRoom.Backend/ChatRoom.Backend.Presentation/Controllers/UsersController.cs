﻿using ChatRoom.Backend.Presentation.ActionFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Users;
using System.Net.Http.Headers;
using Shared.RequestFeatures;

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

        [HttpGet("search/{Name}")]
        [Authorize]
        public async Task<IActionResult> SearchUserByName([FromQuery] UserParameters userParameter, string Name)
        {
            userParameter.Name = Name;
            IEnumerable<UserDto> userDtos = await _service.UserService.SearchUsersByNameAsync(userParameter);
            return Ok(userDtos);
        }

        [HttpPost("{userId}/picture"), DisableRequestSizeLimit]
        [Authorize]
        public async Task<IActionResult> UploadDisplayPicture(int userId) {
            var formCollection = await Request.ReadFormAsync();
            var file = formCollection.Files[0];
            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("Invalid file type. Only image files are allowed.");

            if (file.Length > 0) {
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
                string fileUrl = await _service.FileService.UploadAsync(file.OpenReadStream(), filename, file.ContentType, userId);

                return Ok(fileUrl);
            }
            else {
                return BadRequest();
            }
        }
    }
}
