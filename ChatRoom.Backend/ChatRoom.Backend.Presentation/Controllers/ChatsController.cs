using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Hubs;
using Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.Enums;
using Shared.RequestFeatures;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Shared.DataTransferObjects.Users;
using Shared.DataTransferObjects.Messages;

namespace ChatRoom.Backend.Presentation.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController(IServiceManager service, IHubContext<ChatRoomHub> hubContext) : ControllerBase
    {
        private readonly IServiceManager _service = service;
        private readonly IHubContext<ChatRoomHub> _hubContext = hubContext;

        [HttpGet("{chatId}", Name = "GetChatByChatId")]
        [Authorize]
        public async Task<IActionResult> GetChatById([FromRoute] int chatId)
        {
            ChatDto chat = await _service.ChatService.GetChatByChatIdAsync(chatId);
            if(chat.ChatTypeId == 3)
            {
                throw new ChatNotFoundException(chatId);
            }
            return Ok(chat);
        }

        [HttpGet("{chatId}/members")]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> GetChatMembers(int chatId)
        {
            IEnumerable<ChatMemberDto> chatMembers  = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);
            return Ok(chatMembers);
        }

        [Authorize]
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateChat([FromBody] ChatForCreationDto chat) {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            UserDto userDto = await _service.UserService.GetUserByIdAsync(userId);

            /* Checking if chat already exists for peer to peer chat */
            if (chat.ChatTypeId == (int)ChatTypes.P2P) {
                ChatDto? existingChat = await _service.ChatService.GetP2PChatByUserIdsAsync(chat.ChatMemberIds!.ElementAtOrDefault(0), chat.ChatMemberIds!.ElementAtOrDefault(1));

                if (existingChat != null)
                    return Ok(existingChat);
            }

            ChatDto createdChat = await _service.ChatService.CreateChatWithMembersAsync(chat);

            MessageForCreationDto messageForCreationDto = new MessageForCreationDto
            {
                ChatId = createdChat.ChatId,
                Content = $"{userDto.DisplayName} created the chat.",
                MsgTypeId=(int)MessageTypes.Notification,
                SenderId=userDto.UserId,
            };
            MessageDto messageDto = await _service.MessageService.InsertMessageAsync(messageForCreationDto);

            if (await _service.ChatMemberService.SetIsAdminAsync(createdChat.ChatId, userId, true))
            {
                throw new UserUpdateFailedException(userId);
            }

            /* After chat creation, the system will add all chat members to the signalR group. Including the current user */
            foreach(var member in createdChat.Members!) {
                await _hubContext.Clients.User(member.User!.UserId.ToString()).SendAsync("NewChatCreated", createdChat);
                await _hubContext.Clients.User(member.User!.UserId.ToString()).SendAsync("ReceiveMessage", messageDto);
            }

            return CreatedAtRoute("GetChatByChatId", new { chatId = createdChat.ChatId }, createdChat);
        }

        [Authorize]
        [HttpPut("{chatId:int}/members/{userId:int}/last-seen-message")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateUserLastSeenMessage(int chatId, int userId, [FromBody] ChatMemberForUpdateDto chatMember) {
            ChatMemberDto updatedChatMember = await _service.ChatMemberService.UpdateLastSeenMessageAsync(chatId, userId, chatMember);

            string groupName = ChatRoomHub.GetChatGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("NotifyMessageSeen", updatedChatMember);

            return NoContent();
        }

        [HttpDelete("{chatId}")]
        [Authorize]
        public async Task<IActionResult> Delete(int chatId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            ChatMemberDto member = await _service.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId,userId);
            ChatDto chat = await _service.ChatService.GetChatByChatIdAsync(chatId);

            if(chat.ChatTypeId == 2 && !member.IsAdmin)
            {
                throw new UnauthorizedChatDeletion("Unauthorized Action detected. Access for this action is for chat admins only.");
            }

            if (!(await _service.ChatService.DeleteChatAsync(chatId)))
            {
                throw new ChatNotDeletedException("Something went wrong while deleting the chat. Please try again later");
            }

            string groupName = ChatRoomHub.GetChatGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("DeleteChat", chatId);
            ChatHubChatlistUpdateDto chatHubChatlistUpdateDto = new ChatHubChatlistUpdateDto
            {
                ChatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId),
                Chat = chat
            };
            await _hubContext.Clients.All.SendAsync("ChatlistDeleteChat", chatHubChatlistUpdateDto);
            return Ok();
        }

        [HttpGet("{chatId}/can-view")]
        [Authorize]
        public async Task<bool> CanViewChat(int chatId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            return await _service.ChatService.CanViewAsync(chatId, userId);
        }

        [HttpGet("get-by-user-id")]
        [Authorize]
        public async Task<IActionResult> GetChatListByUserId([FromQuery] ChatParameters chatParameters)
        {
            IEnumerable<ChatDto> chats = await _service.ChatService.GetChatListByChatIdAsync(chatParameters);
            return Ok(chats);
        }

        [HttpGet("{chatId}/typing")]
        [Authorize]
        public async Task<IActionResult> Typing(int chatId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            ChatMemberDto chatMemberDto = await _service.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId);
            string groupName = ChatRoomHub.GetChatGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("UserTyping", chatMemberDto);
            return NoContent();
        }

        [Authorize]
        [HttpPost("display-picture"), DisableRequestSizeLimit]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UploadDisplayPicture() {
            IFormCollection formCollection = await Request.ReadFormAsync();
            IFormFile file = formCollection.Files[0];

            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("Invalid file type. Only image files are allowed.");

            if (file.Length > 0) {
                string filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
                string fileUrl = await _service.FileService.UploadImageAsync(file.OpenReadStream(), filename, file.ContentType, "chat-display-pictures");

                return Ok(fileUrl);
            }
            else {
                return BadRequest();
            }
        }
    }
}
