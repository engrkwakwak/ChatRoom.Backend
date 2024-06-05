using Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Messages
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public int MsgTypeId { get; set; }
        public required string Content { get; set; }
        public DateTime DateSent { get; set; }
        public DateTime DateUpdated { get; set; }
        public int StatusId { get; set; }
    }
}
