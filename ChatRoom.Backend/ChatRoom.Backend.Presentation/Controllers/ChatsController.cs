using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.Enums;

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
            /* Checking if chat already exists for peer to peer chat */
            if(chat.ChatTypeId == (int)ChatTypes.P2P) {
                ChatDto? existingChat = await _service.ChatService.GetP2PChatByUserIdsAsync(chat.ChatMemberIds!.ElementAtOrDefault(0), chat.ChatMemberIds!.ElementAtOrDefault(1));

                if (existingChat != null)
                    return Ok(existingChat);
            }

            ChatDto createdChat = await _service.ChatService.CreateChatWithMembersAsync(chat);

            /* After chat creation, the system will add all chat members to the signalR group. Including the current user */
            foreach(var member in createdChat.Members!) {
                await _hubContext.Clients.User(member.User!.UserId.ToString()).SendAsync("NewChatCreated", createdChat);
            }

            return CreatedAtRoute("GetChatByChatId", new { chatId = createdChat.ChatId }, createdChat);
        }

        [Authorize]
        [HttpPut("{chatId:int}/members/{userId:int}/last-seen-message")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateUserLastSeenMessage(int chatId, int userId, [FromBody] ChatMemberForUpdateDto chatMember) {
            ChatMemberDto updatedChatMember = await _service.ChatMemberService.UpdateLastSeenMessageAsync(chatId, userId, chatMember);

            string groupName = ChatRoomHub.GetGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("NotifyMessageSeen", updatedChatMember);

            return NoContent();
        }
    }
}
