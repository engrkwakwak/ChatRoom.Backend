namespace Shared.DataTransferObjects.Chats {
    public record ChatMemberDto {
        public int ChatId { get; init; }
        public int UserId { get; init; }
        public bool IsAdmin { get; init; }
        public int LastSeenMessageId { get; init; }
        public int StatusId { get; init; }
    }
}
