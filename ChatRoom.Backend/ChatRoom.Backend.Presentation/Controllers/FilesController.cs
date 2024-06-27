using ChatRoom.Backend.Presentation.ActionFilters;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects.File;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase {
        private readonly IFileManager _fileManager;
        public FilesController(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        [Authorize]
        [HttpPost]
        [DisableRequestSizeLimit]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UploadImage([FromForm] PictureForUploadDto picture) {
            if (picture.ImageFile == null || picture.ImageFile.Length == 0)
                return BadRequest("No image provided.");

            if (!picture.ContentType!.StartsWith("image/"))
                return BadRequest("Invalid file type. Only image files are allowed.");

            string fileUrl = await _fileManager.UploadImageAsync(picture.ImageFile.OpenReadStream(), picture);

            return Ok(fileUrl);
        }

        [Authorize]
        [HttpDelete]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> DeleteImage([FromBody] PictureForDeletionDto picture) {
            await _fileManager.DeleteImageAsync(picture.PictureUrl!);

            return NoContent();
        }
    }
}
