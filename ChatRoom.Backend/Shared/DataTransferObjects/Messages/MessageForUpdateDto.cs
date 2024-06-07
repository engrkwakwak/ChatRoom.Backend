using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Messages
{
    public class MessageForUpdateDto
    {
        public int MessageId { get; set; }
        public required string Content { get; set; }
    }
}
