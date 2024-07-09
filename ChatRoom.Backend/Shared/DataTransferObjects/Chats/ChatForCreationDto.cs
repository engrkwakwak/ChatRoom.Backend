using System.ComponentModel.DataAnnotations;
using Shared.CustomValidations;

namespace Shared.DataTransferObjects.Chats {
    public record ChatForCreationDto {
        [Required]
        //[MinimumValue(1)]
        [Range(1, int.MaxValue, ErrorMessage = "SenderId must be a positive integer greater than zero.")]
        public int ChatTypeId { get; init; }

        [Required]
        //[MinimumValue(1)]
        [Range(1, int.MaxValue, ErrorMessage = "SenderId must be a positive integer greater than zero.")]
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
