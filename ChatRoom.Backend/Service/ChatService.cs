using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Users;
using System.Data;

namespace Service {
    internal sealed class ChatService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IChatService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<ChatDto> CreateP2PChatAndAddMembersAsync(int userId1, int userId2)
        {
            Chat? chat = await _repository.Chat.CreateChatAsync(1);
            DataTable userIds = new DataTable("UserIds");
            userIds.Columns.Add("UserId", typeof(int));
            userIds.Rows.Add(userId1);
            userIds.Rows.Add(userId2);
            if(chat == null)
            {
                throw new ChatNotCreatedException("Looks like there was a problem while trying to create the chat. No worries, just give it another shot later on.");
            }

            int affectedRows = await _repository.Chat.AddChatMembersAsync(chat!.ChatId, userIds);
            if (affectedRows < 2)
            {
                throw new AddChatMembersFailedException("There was an issue when trying to add chat members. Let's try that again later.");
            }

            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<ChatDto> CreateChatWithMembersAsync(ChatForCreationDto chatDto) {
            Chat chatEntity = _mapper.Map<Chat>(chatDto);
            Chat? createdChat = await _repository.Chat.CreateChatAsync(chatEntity) ?? throw new ChatNotCreatedException($"{string.Join(",", chatDto.ChatMemberIds!)}");

            createdChat.Members = await _repository.ChatMember.InsertChatMembers(createdChat.ChatId, chatDto.ChatMemberIds!);
            if (createdChat.Members.Count() != chatDto.ChatMemberIds!.Count())
                throw new InsertedChatMemberRowsMismatchException(createdChat.Members.Count(), chatDto.ChatMemberIds!.Count());

            ChatDto chatToReturn = _mapper.Map<ChatDto>(createdChat);
            return chatToReturn;
        }

        public async Task<IEnumerable<UserDto>> GetActiveChatMembersByChatIdAsync(int chatId)
        {
            IEnumerable<User> users = await _repository.Chat.GetActiveChatMembersByChatIdAsync(chatId);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<ChatDto> GetChatByChatIdAsync(int chatId)
        {
            Chat chat = await _repository.Chat.GetChatByChatIdAsync(chatId) ?? throw new ChatNotFoundException(chatId);
            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<int?> GetP2PChatIdByUserIdsAsync(int userId1, int userId2)
        {
            return await _repository.Chat.GetP2PChatIdByUserIdsAsync(userId1, userId2);
        }

        public async Task<IEnumerable<ChatDto>> GetChatsByUserIdAsync(int userId) {
            IEnumerable<Chat> userChats = await _repository.Chat.GetChatsByUserIdAsync(userId);
            IEnumerable<ChatDto> userChatsToReturn = _mapper.Map<IEnumerable<ChatDto>>(userChats);

            return userChatsToReturn;
        }
    }
}
