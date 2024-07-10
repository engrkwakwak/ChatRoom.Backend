using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Auth
{
    public record TokenDto
    {
        public string? Token { get; init; }
    }
}
