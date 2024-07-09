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
using Shared.DataTransferObjects.Users;
using Shared.DataTransferObjects.Messages;
using Contracts;
namespace ChatRoom.Backend.Presentation.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController(IServiceManager service, IHubContext<ChatRoomHub> hubContext, IFileManager fileManager) : ControllerBase
    {
        private readonly IServiceManager _service = service;
        private readonly IHubContext<ChatRoomHub> _hubContext = hubContext;
        private readonly IFileManager _fileManager = fileManager;

        [HttpGet("{chatId}", Name = "GetChatByChatId")]
        [Authorize]
        public async Task<IActionResult> GetChatById([FromRoute] int chatId)
        {
            if (chatId < 1)
            {
                throw new InvalidParameterException("Something went wrong while processing the request. It looks like the chat is invalid.");
            }
            ChatDto chat = await _service.ChatService.GetChatByChatIdAsync(chatId);
            return Ok(chat);
        }

        [HttpGet("{chatId}/members")]
        [Authorize]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> GetChatMembers(int chatId)
        {
            if (chatId < 1)
            {
                throw new InvalidParameterException("Something went wrong while processing the request. It looks like the chat is invalid.");
            }
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

            /* After chat creation, the system will add all chat members to the signalR group. Including the current user */
            IEnumerable<string> memberIds = createdChat.Members!.Select(s => s.User!.UserId.ToString());
            await _hubContext
                .Clients
                .Users(memberIds)
                .SendAsync("NewChatCreated", createdChat);

            if (chat.ChatTypeId == (int)ChatTypes.GroupChat)
            {
                if (!await _service.ChatMemberService.SetIsAdminAsync(createdChat.ChatId, userId, true))
                    throw new UserUpdateFailedException(userId);

                MessageForCreationDto messageForCreationDto = new() {
                    ChatId = createdChat.ChatId,
                    Content = $"{userDto.DisplayName} created the chat.",
                    MsgTypeId=(int)MessageTypes.Notification,
                    SenderId=userDto.UserId,
                };
                MessageDto messageDto = await _service.MessageService.InsertMessageAsync(messageForCreationDto);
                ChatHubChatlistUpdateDto chatHubChatlistUpdateDto = new()
                {
                    Chat = createdChat,
                    LatestMessage = messageDto
                };
                await _hubContext.Clients.Users(memberIds).SendAsync("ChatlistNewMessage", chatHubChatlistUpdateDto);
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
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);
            ChatDto chat = await _service.ChatService.GetChatByChatIdAsync(chatId);

            if(chat.ChatTypeId == 2 && !member.IsAdmin)
            {
                throw new UnauthorizedChatDeletion("Unauthorized Action detected. Access for this action is for chat admins only.");
            }

            if (!(await _service.ChatService.DeleteChatAsync(chatId)))
            {
                throw new ChatNotDeletedException("Something went wrong while deleting the chat. Please try again later");
            }

            if (!string.IsNullOrEmpty(chat.DisplayPictureUrl)) {
                await _fileManager.DeleteImageAsync(chat.DisplayPictureUrl);
            }

            string groupName = ChatRoomHub.GetChatGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("DeleteChat", chatId);
            ChatHubChatlistUpdateDto chatHubChatlistUpdateDto = new() {
                ChatMembers = chatMembers,
                Chat = chat
            };
            IEnumerable<string> memberIds = chatMembers.Select(c => c.User!.UserId.ToString());
            await _hubContext.Clients.Users(memberIds).SendAsync("ChatlistDeleteChat", chatHubChatlistUpdateDto);
            return NoContent();
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

        [HttpGet("{chatId}/typing-start")]
        [Authorize]
        public async Task<IActionResult> TypingStart(int chatId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            ChatMemberDto chatMemberDto = await _service.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId);
            string groupName = ChatRoomHub.GetChatGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("UserStartsTyping", chatMemberDto);
            return NoContent();
        }

        [HttpGet("{chatId}/typing-end")]
        [Authorize]
        public async Task<IActionResult> TypingEnd(int chatId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            ChatMemberDto chatMemberDto = await _service.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, userId);
            string groupName = ChatRoomHub.GetChatGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("UserTypingEnd", chatMemberDto);
            return NoContent();
        }

        [Authorize]
        [HttpPut("{chatId}/leave")]    
        public async Task<IActionResult> Leave(int chatId) 
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            UserDto userDto = await _service.UserService.GetUserByIdAsync(userId);
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);
            ChatMemberDto chatMember = chatMembers.First(m => m.User?.UserId == userId);

            // checks if there are still admins left on the chat
            if (chatMember.IsAdmin && chatMembers.Count(m => m.IsAdmin) <= 1) 
            {
                throw new InvalidParameterException("Invalid request. You cannot leave the chat because you are the only admin left. Please assgin another admin before leaving the chat.");
            }

            if (!await _service.ChatMemberService.SetChatMemberStatus(chatId, userId, (int)Status.Deleted))
            {
                throw new ChatMemberNotUpdatedException(chatId, userId);
            }

            await SendMessageNotification(chatId, $"{userDto.DisplayName} left the Chat.", userId, chatMembers.Where(c => c.User!.UserId != userId));

            return NoContent();
        }

        [HttpPost("{chatId}/add-member/{memberUserId}")]
        [Authorize]
        public async Task<IActionResult> AddMember(int chatId, int memberUserId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            UserDto userDto = await _service.UserService.GetUserByIdAsync(userId);
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);
            ChatMemberDto chatMember = chatMembers.First(m => m.User?.UserId == userId);

            if (!chatMember.IsAdmin)
            {
                throw new UnauthorizedChatActionException("Unauthorized Action detected. Access for this action is for chat admins only.");
            }

            ChatMemberDto member = await _service.ChatMemberService.InsertChatMemberAsync(chatId, memberUserId);

            await _hubContext.Clients.User(memberUserId.ToString()).SendAsync("AddedToChat", chatId);
            await SendMessageNotification(chatId, $"{userDto.DisplayName} added {member.User?.DisplayName} to the group.", userId, chatMembers.Append(member));

            return Ok(member);
        }

        [HttpPost("{chatId}/set-admin/{memberUserId}")]
        [Authorize]
        public async Task<IActionResult> SetAdmin(int chatId, int memberUserId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            UserDto userDto = await _service.UserService.GetUserByIdAsync(userId);
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);
            ChatMemberDto chatMember = chatMembers.First(m => m.User?.UserId == userId);

            if(!chatMember.IsAdmin)
            {
                throw new UnauthorizedChatActionException("Unauthorized Action detected. Access for this action is for chat admins only.");
            }

            if (!await _service.ChatMemberService.SetIsAdminAsync(chatId, memberUserId, true))
            {
                throw new ChatMemberNotUpdatedException(chatId, memberUserId);
            }

            ChatMemberDto newAdminMember = chatMembers.First(m => m.ChatId == chatId && (m.User?.UserId == memberUserId || m.UserId == memberUserId));
            await SendMessageNotification(chatId, $"{userDto.DisplayName} set {newAdminMember.User?.DisplayName} as Admin", userId, chatMembers);
            return NoContent();
        }

        [HttpPost("{chatId}/remove-admin/{memberUserId}")]
        [Authorize]
        public async Task<IActionResult> RemoveAdmin(int chatId, int memberUserId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            UserDto userDto = await _service.UserService.GetUserByIdAsync(userId);
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);
            ChatMemberDto chatMember = chatMembers.First(m => m.User?.UserId == userId);

            if(!chatMember.IsAdmin)
            {
                throw new UnauthorizedChatActionException("Unauthorized Action detected. Access for this action is for chat admins only.");
            }

            if (!await _service.ChatMemberService.SetIsAdminAsync(chatId, memberUserId, false))
            {
                throw new ChatMemberNotUpdatedException(chatId, memberUserId);
            }

            ChatMemberDto newAdminMember = chatMembers.First(m => m.ChatId == chatId && (m.User?.UserId == memberUserId || m.UserId == memberUserId));
            await SendMessageNotification(chatId, $"{userDto.DisplayName} removed {newAdminMember.User?.DisplayName} as Admin", userId, chatMembers);
            return NoContent();
        }

        [HttpDelete("{chatId}/remove-member/{memberUserId}")]
        [Authorize]
        public async Task<IActionResult> RemoveMember(int chatId, int memberUserId)
        {
            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            UserDto userDto = await _service.UserService.GetUserByIdAsync(userId);
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);
            ChatMemberDto chatMember = chatMembers.First(m => m.User?.UserId == userId);

            if (!chatMember.IsAdmin)
                throw new UnauthorizedChatActionException("Unauthorized Action detected. Access for this action is for chat admins only.");

            if(userId == memberUserId)
                throw new UnauthorizedChatActionException("Unauthorized Action detected. You cannot remove yourself from the chat");

            if (!await _service.ChatMemberService.SetChatMemberStatus(chatId, memberUserId, (int)Status.Deleted))
            {
                throw new ChatMemberNotUpdatedException(chatId, memberUserId);
            }

            UserDto removedUser = await _service.UserService.GetUserByIdAsync(memberUserId);
            ChatDto chat = await _service.ChatService.GetChatByChatIdAsync(chatId);
            await _hubContext.Clients.User(removedUser.UserId.ToString()).SendAsync("ChatlistRemovedFromChat", chat);
            await SendMessageNotification(chatId, $"{userDto.DisplayName} removed {removedUser.DisplayName} from the group.", memberUserId, chatMembers.Where(c => c.User!.UserId != removedUser.UserId));
 
            return NoContent();
        }

        [HttpGet("{chatId}/members/{memberUserId}")]
        [Authorize]
        public async Task<IActionResult> Member(int chatId, int memberUserId)
        {
            if (chatId < 1 || memberUserId < 1)
            {
                throw new InvalidParameterException("Something went wrong while fetching the member. The chat id or member id is invalid.");
            }
            ChatMemberDto member = await _service.ChatMemberService.GetChatMemberByChatIdUserIdAsync(chatId, memberUserId);
            return Ok(member);
        }

        private async Task SendMessageNotification(int chatId, string content, int userId, IEnumerable<ChatMemberDto> members)
        {
            MessageForCreationDto messageForCreationDto = new()
            {
                ChatId = chatId,
                Content = content,
                MsgTypeId = (int)MessageTypes.Notification,
                SenderId = userId,
            };
            MessageDto messageDto = await _service.MessageService.InsertMessageAsync(messageForCreationDto);

            ChatHubChatlistUpdateDto chatHubChatlistUpdateDto = new()
            {
                LatestMessage = messageDto,
                Chat = await _service.ChatService.GetChatByChatIdAsync(chatId),
                ChatMembers = members
            };
            string groupName = ChatRoomHub.GetChatGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", messageDto);
            //IEnumerable<string> memberIds = members.Select(m => m.User!.UserId.ToString());
            await _hubContext.Clients.Group(groupName).SendAsync("ChatlistNewMessage", chatHubChatlistUpdateDto);
        }

        [Authorize]
        [HttpPut("{chatId:int}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateChat(int chatId, [FromBody] ChatForUpdateDto chat) {
            if(chatId < 1)
            {
                throw new InvalidParameterException("Something went wrong while fetching the member. The chat id or member id is invalid.");
            }

            await _service.ChatService.UpdateChatAsync(chatId, chat);

            string token = Request.Headers.Authorization[0]!.Replace("Bearer ", "");
            int userId = _service.AuthService.GetUserIdFromJwtToken(token);
            UserDto user = await _service.UserService.GetUserByIdAsync(userId);
            IEnumerable<ChatMemberDto> chatMembers = await _service.ChatMemberService.GetActiveChatMembersByChatIdAsync(chatId);

            await SendMessageNotification(chatId, $"{user.DisplayName} updated the chat name and/or picture", user.UserId, chatMembers);
            
            await _hubContext.Clients.Group(ChatRoomHub.GetChatGroupName(chatId)).SendAsync("UpdateChatNameAndPicture", new { chatId, chat.ChatName, chat.DisplayPictureUrl } );

            return NoContent();
        }
    }
}
