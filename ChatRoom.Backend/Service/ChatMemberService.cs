using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;

namespace Service {
    internal sealed class ChatMemberService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisCacheManager cache) : IChatMemberService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IRedisCacheManager _cache = cache;
        public async Task<ChatMemberDto> UpdateLastSeenMessageAsync(int chatId, int userId, ChatMemberForUpdateDto chatMemberForUpdate) {
            ChatMember chatMemberEntity = await _repository.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId) ?? throw new ChatMemberNotFoundException(chatId, userId);
            if (chatMemberForUpdate.LastSeenMessageId <= chatMemberEntity.LastSeenMessageId) {
                return _mapper.Map<ChatMemberDto>(chatMemberEntity);
            }
            _mapper.Map(chatMemberForUpdate, chatMemberEntity);

            ChatMember updatedChatMember = await _repository.ChatMember.UpdateLastSeenMessageAsync(chatMemberEntity) ?? throw new LastSeenMessageUpdateFailedException(chatMemberEntity.ChatId, chatMemberEntity.UserId);
            ChatMemberDto chatMemberToReturn = _mapper.Map<ChatMemberDto>(updatedChatMember);
            return chatMemberToReturn;
        }

        public async Task<IEnumerable<ChatMemberDto>> GetActiveChatMembersByChatIdAsync(int chatId) {
            string chatKey = $"chat:{chatId}:activeChatMembers";
            IEnumerable<ChatMember> chatMembers = await _cache.GetCachedDataAsync<IEnumerable<ChatMember>>(chatKey);
            if(chatMembers != null)
            {
                return _mapper.Map<IEnumerable<ChatMemberDto>>(chatMembers);
            }
            chatMembers = await _repository.ChatMember.GetActiveChatMembersByChatIdAsync(chatId);
            _cache.SetCachedData(chatKey, chatMembers, TimeSpan.FromMinutes(30));
            return _mapper.Map<IEnumerable<ChatMemberDto>>(chatMembers);
        }

        public async Task<ChatMemberDto> GetChatMemberByChatIdUserIdAsync(int chatId, int userId)
        {
            string chatMemberKey = $"chatMember:userId:{userId}:chatId:{chatId}";
            ChatMember? chatMember = await _cache.GetCachedDataAsync<ChatMember>(chatMemberKey);
            if(chatMember != null )
            {
                _mapper.Map<ChatMemberDto>(chatMember);
            }

            chatMember = await _repository.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId) ?? throw new ChatMemberNotFoundException(chatId, userId); ;
            _cache.SetCachedData(chatMemberKey, chatMember, TimeSpan.FromMinutes(30));
            return _mapper.Map<ChatMemberDto>(chatMember);
        }

        public async Task<bool> SetIsAdminAsync(int chatId, int userId, bool isAdmin)
        {
            int affectedRows = await _repository.ChatMember.SetIsAdminAsync(chatId, userId, isAdmin);
            string chatMemberKey = $"chatMember:userId:{userId}:chatId:{chatId}";
            await _cache.RemoveDataAsync(chatMemberKey);
            await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
            return affectedRows > 0;
        }

        public async Task<bool> SetChatMemberStatus(int chatId, int userId, int statusId)
        {
            int affectedRows = await _repository.ChatMember.SetChatMemberStatus(chatId, userId, statusId);
            await _cache.RemoveDataAsync($"chatMember:userId:{userId}:chatId:{chatId}");
            await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
            return affectedRows > 0;
        }

        public async Task<ChatMemberDto[]> InsertChatMembersAsync(int chatId, IEnumerable<int> userIds)
        {
            IEnumerable<ChatMember> chatMembers = await _repository.ChatMember.InsertChatMembers(chatId, userIds);
            await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
            return _mapper.Map<ChatMemberDto[]>(chatMembers);
        }

        public async Task<ChatMemberDto> InsertChatMemberAsync(int chatId, int userId)
        {
            ChatMember? member = await _repository.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId);
            if(member == null)
            {
                IEnumerable<int> userIds = [userId];
                await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
                IEnumerable<ChatMember> chatMembers = await _repository.ChatMember.InsertChatMembers(chatId, userIds);
                return _mapper.Map<ChatMemberDto>(chatMembers.First());
            }
            await _repository.ChatMember.SetChatMemberStatus(chatId, userId, 1);
            await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
            member.StatusId = 1;
            return _mapper.Map<ChatMemberDto>(member);
        }
    }
}
