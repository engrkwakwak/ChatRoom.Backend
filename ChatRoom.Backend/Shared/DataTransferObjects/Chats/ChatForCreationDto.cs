using System.ComponentModel.DataAnnotations;
using Shared.CustomValidations;

namespace Shared.DataTransferObjects.Chats {
    public record ChatForCreationDto {
        [Required]
        //[MinimumValue(1)]
        [Range(1, 2, ErrorMessage = "SenderId must be in range 1 to 2.")]
        public int ChatTypeId { get; init; }

        [Required]
        //[MinimumValue(1)]
        [Range(1, 3, ErrorMessage = "StatusId must be in range 1 to 3.")]
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
