using Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.ChatMembers
{
    public class ChatMemberDto
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public bool IsAdmin { get; set; }
        public int LastSeenMessageId { get; set; }
        public int StatusId { get; set; }
    }
}
