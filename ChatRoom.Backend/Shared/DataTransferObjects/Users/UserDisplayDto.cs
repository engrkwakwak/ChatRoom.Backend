namespace Shared.DataTransferObjects.Users {
    public record UserDisplayDto {
        public int UserId { get; init; }
        public string DisplayName { get; init; } = String.Empty;
        public string DisplayPictureUrl { get; set; } = String.Empty;
    }
}
