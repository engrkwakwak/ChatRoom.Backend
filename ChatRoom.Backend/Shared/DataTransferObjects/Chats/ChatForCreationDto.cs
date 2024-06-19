using System.ComponentModel.DataAnnotations;
using Shared.CustomValidations;

namespace Shared.DataTransferObjects.Chats {
    public record ChatForCreationDto {
        [Required]
        [MinimumValue(1)]
        public int ChatTypeId { get; init; }

        [Required]
        [MinimumValue(1)]
        public int StatusId { get; init; }

        [MaxLength(50)]
        public string? ChatName { get; init; }

        [MaxLength(200)]
        public string? DisplayPictureUrl { get; init; }

        [Required]
        [MinimumElements(2)]
        [AllMembersGreaterThanZero]
        public IEnumerable<int>? ChatMemberIds { get; init; }
    }
}
