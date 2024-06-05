using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Messages
{
    public class MessageForCreationDto
    {
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public int MsgTypeId { get; set; }
        public required string Content { get; set; }
    }
}
