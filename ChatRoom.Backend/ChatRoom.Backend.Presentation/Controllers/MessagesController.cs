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
        //[Authorize]
        public async Task<IActionResult> GetMessagesForChat(int chatId, [FromQuery] MessageParameters messageParameters) {
            (IEnumerable<MessageDto> messages, MetaData? metaData) = await _service.MessageService.GetMessagesByChatIdAsync(messageParameters, chatId);
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData));

            return Ok(messages);
        }

        [HttpPost("")]
        public async Task<IActionResult> SendMessage([FromBody] MessageForCreationDto message)
        {
            message.MsgTypeId = 1;
            MessageDto createdMessage = await _service.MessageService.InsertMessageAsync(message);
            return Ok(createdMessage);
        }
    }
}
