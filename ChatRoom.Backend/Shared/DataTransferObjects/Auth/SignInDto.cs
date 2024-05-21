using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Auth {
    public record SignInDto {
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(20, ErrorMessage = "Maximum length for the Username is 20 characters.")]
        public string? Username { get; init; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; init; }
    }
}
