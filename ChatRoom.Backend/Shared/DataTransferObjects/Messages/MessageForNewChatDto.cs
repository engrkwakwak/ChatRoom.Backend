using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Messages
{
    public class MessageForNewChatDto
    {
        [Required]
        public int? ReceiverId { get; set; }

        [Required]  
        public int? SenderId { get; set;}

        [Required]
        [StringLength(5000)]
        public string? Content { get; set; }
    }
}
