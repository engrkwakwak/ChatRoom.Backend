namespace Shared.DataTransferObjects.Chats
{
    public record ChatDto
    {
        public int ChatId { get; init; }
        public int ChatTypeId { get; init; }
        public string? ChatName { get; init; }
        public DateTime? DateCreated { get; init; }
        public int StatusId { get; init; }

        public IEnumerable<ChatMemberDto>? Members { get; init; }
    }
}
