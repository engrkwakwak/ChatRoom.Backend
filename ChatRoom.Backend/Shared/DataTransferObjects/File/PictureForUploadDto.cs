using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.File {
    public record PictureForUploadDto {
        [Required]
        public string? FileName { get; init; }

        [Required]
        public string? ContentType { get; init; }

        [Required]
        public string? ContainerName { get; init; }

        public IFormFile? ImageFile { get; init; }
    }
}
