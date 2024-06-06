using Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
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
            (IEnumerable<MessageDto> messages, MetaData? metaData) = await _service.MessageService.GetMessagesByChatIdAsync(messageParameters, chatId);
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData));

            return Ok(messages);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] MessageForCreationDto message)
        {
            message.MsgTypeId = 1;
            MessageDto createdMessage = await _service.MessageService.InsertMessageAsync(message);

            // emit signalR here

            return Ok(createdMessage);
        }

        [HttpDelete("{messageId}")]
        [Authorize]
        public async Task<IActionResult> Delete(int messageId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            MessageDto message = await _service.MessageService.GetMessageByMessageIdAsync(messageId);
            if(message.Sender?.UserId != userId)
            {
                throw new UnauthorizedMessageDeletionException("Deleting messages sent by other users are strictly prohibited.");
            }
            if (!await _service.MessageService.DeleteMessageAsync(messageId))
            {
                throw new MessageUpdateFailedException("Something went wrong while deleting the message. Please try again later.");
            }
            return Ok();
        }

        [HttpPut("{messageId}")]
        [Authorize]
        public async Task<IActionResult> Update(int messageId)
        {
            return Ok("Updated");
        }
    }
}
