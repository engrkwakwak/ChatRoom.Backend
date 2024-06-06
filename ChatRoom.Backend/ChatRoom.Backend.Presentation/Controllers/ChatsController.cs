

using ChatRoom.Backend.Presentation.ActionFilters;
using ChatRoom.Backend.Presentation.Hubs;
using Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Messages;
using Shared.Utilities;

namespace ChatRoom.Backend.Presentation.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController(IServiceManager service, IHubContext<ChatRoomHub> hubContext) : ControllerBase
    {
        private readonly IServiceManager _service = service;
        private readonly IHubContext<ChatRoomHub> _hubContext = hubContext;

        [HttpGet("get-p2p-chatid-by-userids")]
        [Authorize]
        public async Task<IActionResult> GetP2PChatByUserIdPair(int userId1, int userId2)
        {
            if (userId1 < 1 || userId2 < 1)
            {
                throw new InvalidParameterException("Something went wrong. Invalid inputs was detected.");
            }
            int? chatId = await _service.ChatService.GetP2PChatIdByUserIdsAsync(userId1, userId2);
            return Ok(chatId);
        }

        [HttpGet("{chatId}", Name = "GetChatByChatId")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] int chatId)
        {
            ChatDto chat = await _service.ChatService.GetChatByChatIdAsync(chatId);
            return Ok(chat);
        }

        [HttpGet("{chatId}/members")]
        [Authorize]
        public async Task<IActionResult> GetMembers([FromRoute] int chatId)
        {
            if(chatId < 1)
            {
                throw new InvalidParameterException("Something went wrong with your request. Please try again later.");
            }
            return Ok(await _service.ChatService.GetActiveChatMembersByChatIdAsync(chatId));
        }

        [HttpPost("send-message-to-new-chat")]
        [Authorize]
        public async Task<IActionResult> SendMessageToNewChat([FromBody] MessageForNewChatDto message)
        {
            ChatDto chatDto;
            MessageDto createdMessage;
            MessageForCreationDto messageForCreationDto = new MessageForCreationDto
            {
                MsgTypeId = 1,
                ChatId = 0,
                Content = message.Content!,
                SenderId = (int)message.SenderId!
            };

            int? chatId = await _service.ChatService.GetP2PChatIdByUserIdsAsync((int)message.SenderId!, (int)message.ReceiverId!);
            if (chatId != null)
            {
                chatDto = await _service.ChatService.GetChatByChatIdAsync((int)chatId);
                messageForCreationDto.ChatId = chatDto.ChatId;
                createdMessage = await _service.MessageService.InsertMessageAsync(messageForCreationDto);
            }
            else
            {
                chatDto = await _service.ChatService.CreateP2PChatAndAddMembersAsync((int)message.SenderId!, (int)message.ReceiverId!);
                ContactForCreationDto contact = new ContactForCreationDto
                {
                    ContactId = message.ReceiverId,
                    UserId = message.SenderId,
                    StatusId = 2,
                };
                if(!await _service.ContactService.InsertOrUpdateContactAsync(contact))
                {
                    throw new ContactsNotCreatedException("Something went wrong while adding the user as your contacts.");
                }
                messageForCreationDto.ChatId = chatDto.ChatId;
                createdMessage = await _service.MessageService.InsertMessageAsync(messageForCreationDto);
                
            }

            // emit signalR here

            return Ok(createdMessage);
        }

        [Authorize]
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateChat([FromBody] ChatForCreationDto chat) {
            /* Checking if chat already exists for peer to peer chat */
            if(chat.ChatTypeId == (int)EnumHelper.ChatTypes.P2P) {
                ChatDto? existingChat = await _service.ChatService.GetP2PChatByUserIdsAsync(chat.ChatMemberIds!.ElementAtOrDefault(0), chat.ChatMemberIds!.ElementAtOrDefault(1));

                if (existingChat != null)
                    return Ok(existingChat);
            }

            ChatDto createdChat = await _service.ChatService.CreateChatWithMembersAsync(chat);

            /* After chat creation, the system will add all chat members to the signalR group via frontend. Including the current user */
            foreach(var member in createdChat.Members!) {
                await _hubContext.Clients.User(member.UserId.ToString()).SendAsync("NewChatCreated", createdChat);
            }

            return CreatedAtRoute("GetChatByChatId", new { chatId = createdChat.ChatId }, createdChat);
        }
    }
}
