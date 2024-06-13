using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Messages;
using Shared.DataTransferObjects.Users;
using Shared.Enums;
using Shared.RequestFeatures;
using System.Text.Json;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/chats/{chatId}/messages")]
    [ApiController]
    public class MessagesController(IServiceManager service, IHubContext<ChatRoomHub> hubContext) : ControllerBase {
        private readonly IServiceManager _service = service;
        private readonly IHubContext<ChatRoomHub> _hubContext = hubContext;

        [HttpGet]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> GetMessagesForChat(int chatId, [FromQuery] MessageParameters messageParameters) {
            (IEnumerable<MessageDto> messages, MetaData? metaData) = await _service.MessageService.GetMessagesByChatIdAsync(messageParameters, chatId);
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData));

            return Ok(messages);
        }

        [HttpPost]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> SendMessage([FromBody] MessageForCreationDto message)
        {
            //check chatId exists
            ChatDto currentChat = await _service.ChatService.GetChatByChatIdAsync(message.ChatId);
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(message.ChatId);

            //insert to contacts automatically if p2p
            if(currentChat.ChatTypeId == (int)ChatTypes.P2P) {
                IEnumerable<ContactDto> chatContacts = await _service.ContactService.InsertContactsAsync(message.SenderId, chatMembers.Where(u => u.User!.UserId != message.SenderId).Select(u => u.User!.UserId).ToList());
            }
            

            MessageDto createdMessage = await _service.MessageService.InsertMessageAsync(message);
            string groupName = ChatRoomHub.GetGroupName(createdMessage.ChatId);

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", createdMessage);

            return Ok(createdMessage);
        }
    }
}
