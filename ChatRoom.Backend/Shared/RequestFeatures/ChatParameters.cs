using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
    public class ChatParameters : RequestParameters
    {
        [Required]
        public required string UserId { get; set; }
        public string? Name { get; set; }
    }
}
