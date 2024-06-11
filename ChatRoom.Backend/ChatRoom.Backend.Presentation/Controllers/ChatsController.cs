﻿

using Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Messages;
using Shared.RequestFeatures;

namespace ChatRoom.Backend.Presentation.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController(IServiceManager service) : ControllerBase
    {
        private readonly IServiceManager _service = service;

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

        [HttpGet("{chatId}")]
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

        [HttpGet("get-by-user-id")]
        [Authorize]
        public async Task<IActionResult> GetChatListByUserId([FromQuery] ChatParameters chatParameters)
        {
            IEnumerable<ChatDto> chats = await _service.ChatService.GetChatListByChatIdAsync(chatParameters);
            return Ok(chats);
        }

    }
}
