namespace Shared.DataTransferObjects.Messages {
    public record MessageTypeDto {
        public int MsgTypeId { get; init; }
        public string MsgTypeName { get; init; } = String.Empty;
    }
}
