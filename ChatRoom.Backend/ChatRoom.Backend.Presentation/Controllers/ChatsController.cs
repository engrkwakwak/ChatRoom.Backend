﻿using ChatRoom.Backend.Presentation.ActionFilters;
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
using System.Runtime.InteropServices;
using Entities.Models;

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

            if (await _service.ChatMemberService.SetChatMemberStatus(chatId, userId, (int) Shared.Enums.Status.Deleted)){
                throw new ChatMemberNotUpdatedException(chatId, userId);
            }

            await SendMessageNotification(chatId, $"{userDto.DisplayName} left the Chat.", userId, chatMembers);

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

            await SendMessageNotification(chatId, $"{userDto.DisplayName} added {member.User?.DisplayName} to the group.", userId, chatMembers);

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

        private async Task SendMessageNotification(int chatId, string content, int userId, IEnumerable<ChatMemberDto> members)
        {
            MessageForCreationDto messageForCreationDto = new MessageForCreationDto
            {
                ChatId = chatId,
                Content = content,
                MsgTypeId = (int)MessageTypes.Notification,
                SenderId = userId,
            };
            MessageDto messageDto = await _service.MessageService.InsertMessageAsync(messageForCreationDto);
            string groupName = ChatRoomHub.GetChatGroupName(chatId);
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", messageDto);

            ChatHubChatlistUpdateDto chatHubChatlistUpdateDto = new ChatHubChatlistUpdateDto
            {
                LatestMessage = messageDto,
                Chat = await _service.ChatService.GetChatByChatIdAsync(chatId),
                ChatMembers = members
            };
            foreach (ChatMemberDto member in members)
            {
                await _hubContext.Clients.User(member.User!.UserId.ToString()).SendAsync("ChatlistNewMessage", chatHubChatlistUpdateDto);
            }
        }
    }
}
