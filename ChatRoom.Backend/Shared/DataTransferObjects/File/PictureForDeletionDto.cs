using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.File {
    public record PictureForDeletionDto {

        [Required]
        public string? PictureUrl { get; init; }
    }
}
