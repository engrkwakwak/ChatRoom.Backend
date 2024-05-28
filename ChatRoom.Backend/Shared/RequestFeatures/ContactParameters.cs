using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
    public class ContactParameters : RequestParameters
    {
        [Required]
        public required int UserId { get; set; }
    }
}
