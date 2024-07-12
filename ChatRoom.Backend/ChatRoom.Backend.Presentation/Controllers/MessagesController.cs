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
            (IEnumerable<MessageDto> messages, MetaData? metaData) = await _service.MessageService.GetMessagesByChatIdAsync(messageParameters, chatId);
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData));

            return Ok(messages);
        }

        [HttpPost]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> SendMessage([FromBody] MessageForCreationDto message) {
            //check chatId exists
            ChatDto currentChat = await _service.ChatService.GetChatByChatIdAsync(message.ChatId);
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(message.ChatId);
            if (!chatMembers.Any()) {
                throw new NoChatMembersFoundException(currentChat.ChatId);
            }

            //insert to contacts automatically if p2p
            if (currentChat.ChatTypeId == (int)ChatTypes.P2P) {
                IEnumerable<int> memberIds = chatMembers.Where(u => u.User!.UserId != message.SenderId).Select(u => u.User!.UserId);
                IEnumerable<ContactDto> chatContacts = await _service.ContactService.InsertContactsAsync(message.SenderId, memberIds.ToList());
                await _hubContext.Clients.User(message.SenderId.ToString()).SendAsync("ContactsUpdated");
            }

            MessageDto createdMessage = await _service.MessageService.InsertMessageAsync(message);
            string groupName = ChatRoomHub.GetChatGroupName(createdMessage.ChatId);

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", createdMessage);
            ChatHubChatlistUpdateDto chatHubChatlistUpdateDto = new() {
                LatestMessage = createdMessage,
                Chat = currentChat,
                ChatMembers = chatMembers
            };
            await _hubContext.Clients.Group(groupName).SendAsync("ChatlistNewMessage", chatHubChatlistUpdateDto);

            return Ok(createdMessage);
        }

        [HttpGet("latest")]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> GetLatestMessage(int chatId) {
            MessageParameters messageParameters = new() {
                PageNumber = 1,
                PageSize = 1,
            };
            (IEnumerable<MessageDto> messages, MetaData? metaData) = await _service.MessageService.GetMessagesByChatIdAsync(messageParameters, chatId);

            return Ok(messages.FirstOrDefault());
        }

        [HttpDelete("{messageId}")]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Delete(int messageId) {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            if (!await IsAuthorized(token, messageId)) {
                throw new UnauthorizedMessageDeletionException("Deleting messages sent by other users are strictly prohibited.");
            }

            MessageDto deletedMessage = await _service.MessageService.DeleteMessageAsync(messageId);

            string groupName = ChatRoomHub.GetChatGroupName(deletedMessage.ChatId);
            await _hubContext.Clients.Group(groupName).SendAsync("DeleteMessage", deletedMessage);

            return NoContent();
        }

        [HttpPut("{messageId}")]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Update(int messageId, MessageForUpdateDto message) {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            if (!await IsAuthorized(token, messageId)) {
                throw new UnauthorizedMessageDeletionException("Updating messages sent by other users are strictly prohibited.");
            }

            MessageDto updatedMessage = await _service.MessageService.UpdateMessageAsync(message);

            string groupName = ChatRoomHub.GetChatGroupName(updatedMessage.ChatId);
            await _hubContext.Clients.Group(groupName).SendAsync("UpdateMessage", updatedMessage);

            return Ok(updatedMessage);
        }

        private async Task<bool> IsAuthorized(string token, int messageId) {
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            MessageDto message = await _service.MessageService.GetMessageByMessageIdAsync(messageId);

            return message.Sender!.UserId == userId;
        }
    }
}
