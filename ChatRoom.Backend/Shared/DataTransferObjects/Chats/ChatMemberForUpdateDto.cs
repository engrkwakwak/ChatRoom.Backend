using Shared.CustomValidations;

namespace Shared.DataTransferObjects.Chats {
    public class ChatMemberForUpdateDto {
        public bool IsAdmin { get; init; }

        [MinimumValue(1)]
        public int LastSeenMessageId { get; init; }

        [MinimumValue(1)]
        public int StatusId { get; init; }
    }
}
