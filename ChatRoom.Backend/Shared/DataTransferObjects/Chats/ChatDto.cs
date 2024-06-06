using Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Chats
{
    public class ChatDto
    {
        public int ChatId { get; set; }
        public int ChatTypeId { get; set; }
        public string? ChatName { get; set; }
        public string? DisplayPictureUrl { get; set; }
        public DateTime? DateCreated { get; set; }
        public int StatusId { get; set; }
    }
}
