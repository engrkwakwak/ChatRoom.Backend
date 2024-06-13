using Shared.CustomValidations;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Messages {
    public class MessageForCreationDto {
        [Required]
        [MinimumValue(1)]
        public int ChatId { get; set; }

        [Required]
        [MinimumValue(1)]
        public int SenderId { get; set; }

        [Required]
        [MinimumValue(1)]
        public int MsgTypeId { get; set; }

        [Required]
        public string? Content { get; set; }
    }
}