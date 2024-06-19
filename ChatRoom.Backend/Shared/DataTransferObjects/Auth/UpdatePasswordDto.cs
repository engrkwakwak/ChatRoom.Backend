using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Auth
{
    public record UpdatePasswordDto
    {
        public string Token {  get; set; } 
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
    }
}
