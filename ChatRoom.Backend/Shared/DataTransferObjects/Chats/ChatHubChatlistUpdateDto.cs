using Shared.DataTransferObjects.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Chats
{
    public record ChatHubChatlistUpdateDto
    {
        public IEnumerable<ChatMemberDto> ChatMembers { get; init; } = [];

        public MessageDto? LatestMessage { get; init; }

        public ChatDto? Chat { get; init; }


    }
}
