using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Users;
using System.Data;

namespace Service {
    internal sealed class ChatService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisCacheManager cache) : IChatService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IRedisCacheManager _cache = cache;

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
            
            string chatlistCacheKey1 = $"user:{userId1}:chats:page:1";
            string chatlistCacheKey2 = $"user:{userId2}:chats:page:1";
            await _cache.RemoveDataAsync(chatlistCacheKey1);
            await _cache.RemoveDataAsync(chatlistCacheKey2);
            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<IEnumerable<UserDto>> GetActiveChatMembersByChatIdAsync(int chatId)
        {
            string chatUsersCacheKey = $"chat:{chatId}:users";
            IEnumerable<User> users = await _cache.GetCachedDataAsync<IEnumerable<User>>(chatUsersCacheKey);
            if(users != null)
                return _mapper.Map<IEnumerable<UserDto>>(users);
            
            users = await _repository.Chat.GetActiveChatMembersByChatIdAsync(chatId);
            _cache.SetCachedData(chatUsersCacheKey, users, TimeSpan.FromMinutes(30));
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<ChatDto> GetChatByChatIdAsync(int chatId)
        {
            string chatCacheKey = $"chat:{chatId}";
            Chat? chat = await _cache.GetCachedDataAsync<Chat>(chatCacheKey);
            if(chat != null)
                return _mapper.Map<ChatDto>(chat);

            chat = await _repository.Chat.GetChatByChatIdAsync(chatId);
            if (chat == null)
                throw new ChatNotFoundException("An error occurred while retrieving the chat messages. Please try again later.");
            _cache.SetCachedData(chatCacheKey, chat, TimeSpan.FromMinutes(30));
            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<int?> GetP2PChatIdByUserIdsAsync(int userId1, int userId2)
        {
            return await _repository.Chat.GetP2PChatIdByUserIdsAsync(userId1, userId2);
        }
    }
}
