namespace Shared.DataTransferObjects.Users
{
    public class UserDto
    {
        public int UserId { get; set; }

        public required string Username { get; set; }

        public required string DisplayName { get; set; }

        public required string Email { get; set; }

        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool IsEmailVerified { get; set; }

        public string? DisplayPictureUrl { get; set; }

        public DateTime? DateCreated { get; set; }
    }
}
