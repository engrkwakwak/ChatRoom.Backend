using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Users {
    public class UserForUpdateDto {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, ErrorMessage = "The maximum length for 'username' is 20 characters.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Display name is required.")]
        [StringLength(50, ErrorMessage = "The maximum length for 'display name' is 50 characters.")]
        public string? DisplayName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100, ErrorMessage = "The maximum length for 'email' is 100 characters.")]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100, ErrorMessage = "The maximum length for 'address' is 100 characters.")]
        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(200, ErrorMessage = "The maximum length for 'picture path' is 200 characters.")]
        public string? DisplayPictureUrl { get; set; }
    }
}
