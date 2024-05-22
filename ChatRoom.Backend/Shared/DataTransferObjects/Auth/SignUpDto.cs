using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Auth
{
    public class SignUpDto
    {
        [Required]
        [StringLength(50)]
        public required string DisplayName { get; set; }

        [Required]
        [StringLength(20)]
        public required string Username { get; set; }

        [Required]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        public required string Password { get; set; }

        [Required]
        [MinLength(8)]
        public required string PasswordConfirmation { get; set; }

    }
}
