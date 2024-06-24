using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Chats {
    public record ChatForUpdateDto {
        [MaxLength(50)]
        public string? ChatName { get; init; }

        [MaxLength(200)]
        public string? DisplayPictureUrl { get; init; }
    }
}
