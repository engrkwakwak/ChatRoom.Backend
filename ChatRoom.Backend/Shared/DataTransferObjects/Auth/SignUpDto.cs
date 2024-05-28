using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Auth
{
    public class SignUpDto
    {
        [Required]
        [StringLength(50)]
        public  string? DisplayName { get; set; }

        [Required]
        [StringLength(20)]
        public  string? Username { get; set; }

        [Required]
        [StringLength(100)]
        public  string? Email { get; set; }

        [Required]
        [MinLength(8)]
        public  string? Password { get; set; }

        [Required]
        [MinLength(8)]
        public  string? PasswordConfirmation { get; set; }

    }
}
