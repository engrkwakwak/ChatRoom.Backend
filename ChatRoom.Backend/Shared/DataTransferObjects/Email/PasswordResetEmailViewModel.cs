using Microsoft.AspNetCore.Http;
using Shared.DataTransferObjects.Users;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Email
{
    public class PasswordResetEmailViewModel
    {
        [Required]
        public UserDto User {  get; set; }
        public string PasswordResetLink { get; set; }
    }
}
