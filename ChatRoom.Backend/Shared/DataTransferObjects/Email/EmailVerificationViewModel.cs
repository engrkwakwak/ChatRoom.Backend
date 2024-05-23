using Microsoft.AspNetCore.Http;
using Shared.DataTransferObjects.Users;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.Email
{
    public class EmailVerificationViewModel
    {
        [Required]
        public UserDto User {  get; set; }
        public string VerificationLink { get; set; }
    }
}
