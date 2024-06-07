using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Messages;
using Shared.RequestFeatures;
using System.Text.Json;

namespace ChatRoom.Backend.Presentation.Controllers {
    [Route("api/chats/{chatId}/messages")]
    [ApiController]
    public class MessagesController(IServiceManager service) : ControllerBase {
        private readonly IServiceManager _service = service;

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMessagesForChat(int chatId, [FromQuery] MessageParameters messageParameters) {
            await CheckChatExistance(chatId);
            (IEnumerable<MessageDto> messages, MetaData? metaData) = await _service.MessageService.GetMessagesByChatIdAsync(messageParameters, chatId);
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData));

            return Ok(messages);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] MessageForCreationDto message, int chatId)
        {
            message.MsgTypeId = 1;

            await CheckChatExistance(chatId);

            MessageDto createdMessage = await _service.MessageService.InsertMessageAsync(message);

            // emit signalR here

            return Ok(createdMessage);
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
            return Ok();
        }

        [HttpPut("{messageId}")]
        [Authorize]
        public async Task<IActionResult> Update(int messageId, MessageForUpdateDto message)
        {
            await AuthorizedAction(Request.Headers.Authorization[0]!.Replace("Bearer ", ""), messageId);
            MessageDto updatedMessage = await _service.MessageService.UpdateMessageAsync(message);
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
                throw new ChatNotFoundException("The conversation you are trying to access may have been deleted and doesnt exist anymore.");
            }
        }
    }
}
