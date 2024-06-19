using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Hubs;
using Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Messages;
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
            await CheckChatExistance(chatId);
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
            string groupName = ChatRoomHub.GetChatGroupName(createdMessage.ChatId);

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", createdMessage);
            ChatHubChatlistUpdateDto chatHubChatlistUpdateDto = new ChatHubChatlistUpdateDto
            {
                LatestMessage = createdMessage,
                Chat = currentChat,
                ChatMembers = chatMembers
            };
            await _hubContext.Clients.All.SendAsync("ChatlistNewMessage", chatHubChatlistUpdateDto);

            return Ok(createdMessage);
        }

        [HttpGet("latest")]
        [Authorize]
        public async Task<IActionResult> GetLatestMessage(int chatId)
        {
            MessageParameters messageParameters = new MessageParameters
            {
                PageNumber = 1,
                PageSize = 1,
            };
            (IEnumerable<MessageDto> messages, MetaData? metaData) = await _service.MessageService.GetMessagesByChatIdAsync(messageParameters, chatId);

            return Ok(messages.First());
        }

        [HttpDelete("{messageId}")]
        [Authorize]
        public async Task<IActionResult> Delete(int messageId)
        {
            await AuthorizedAction(Request.Headers.Authorization[0]!.Replace("Bearer ", ""), messageId);
            if (!await _service.MessageService.DeleteMessageAsync(messageId))
            {
                throw new MessageUpdateFailedException("Something went wrong while deleting the message. Please try again later.");
            }
            MessageDto deletedMessage = await _service.MessageService.GetMessageByMessageIdAsync(messageId);
            string groupName = ChatRoomHub.GetChatGroupName(deletedMessage.ChatId);
            await _hubContext.Clients.Group(groupName).SendAsync("DeleteMessage", deletedMessage);
            return Ok();
        }

        [HttpPut("{messageId}")]
        [Authorize]
        public async Task<IActionResult> Update(int messageId, MessageForUpdateDto message)
        {
            await AuthorizedAction(Request.Headers.Authorization[0]!.Replace("Bearer ", ""), messageId);
            MessageDto updatedMessage = await _service.MessageService.UpdateMessageAsync(message);
            string groupName = ChatRoomHub.GetChatGroupName(updatedMessage.ChatId);
            await _hubContext.Clients.Group(groupName).SendAsync("UpdateMessage", updatedMessage);
            return Ok(updatedMessage);
        }
        
        private async Task AuthorizedAction(string token, int messageId)
        {
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            MessageDto message = await _service.MessageService.GetMessageByMessageIdAsync(messageId);
            if (message.Sender?.UserId != userId)
            {
                throw new UnauthorizedMessageDeletionException("Deleting messages sent by other users are strictly prohibited.");
            }
        }

        private async Task CheckChatExistance(int chatId)
        {
            ChatDto chatDto = await _service.ChatService.GetChatByChatIdAsync(chatId);
            if (chatDto.StatusId == 3)
            {
                throw new ChatNotFoundException(chatId);
            }
        }
    }
}
