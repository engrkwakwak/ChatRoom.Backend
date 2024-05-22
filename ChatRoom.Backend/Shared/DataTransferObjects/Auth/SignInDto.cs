using Shared.CustomValidations;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Auth {
    public record SignInDto {
        [Required(ErrorMessage = "Username is required.")]
        [UsernameOrEmail]
        public string? Username { get; init; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; init; }
    }
}
