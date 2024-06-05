using Shared.DataTransferObjects.Status;
using Shared.DataTransferObjects.Users;

namespace Shared.DataTransferObjects.Messages {
    public record MessageDto {
        public int MessageId { get; init; }
        public int ChatId { get; init; }
        public string Content { get; init; } = String.Empty;
        public DateTime DateSent { get; init; } = DateTime.Now;
        public UserDisplayDto? Sender { get; init; }
        public MessageTypeDto? MessageType { get; init; }
        public StatusDto? Status { get; init; }
    }
}
