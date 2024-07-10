using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.IdentityModel.Tokens;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Users;

namespace Service {
    internal sealed class ChatMemberService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisCacheManager cache) : IChatMemberService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IRedisCacheManager _cache = cache;
        public async Task<ChatMemberDto> UpdateLastSeenMessageAsync(int chatId, int userId, ChatMemberForUpdateDto chatMemberForUpdate) {
            if(userId < 1 || chatId < 1)
            {
                throw new InvalidParameterException($"Invalid chatId : {chatId} or userId : {userId}");
            }
            ChatMember chatMemberEntity = await _repository.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId) ?? throw new ChatMemberNotFoundException(chatId, userId);
            if (chatMemberForUpdate.LastSeenMessageId <= chatMemberEntity.LastSeenMessageId) {
                return _mapper.Map<ChatMemberDto>(chatMemberEntity);
            }
            _mapper.Map(chatMemberForUpdate, chatMemberEntity);

            ChatMember updatedChatMember = await _repository.ChatMember.UpdateLastSeenMessageAsync(chatMemberEntity) ?? throw new LastSeenMessageUpdateFailedException(chatMemberEntity.ChatId, chatMemberEntity.UserId);
            ChatMemberDto chatMemberToReturn = _mapper.Map<ChatMemberDto>(updatedChatMember);

            string chatKey = $"chat:{chatId}:activeChatMembers";
            await _cache.RemoveDataAsync(chatKey);
            return chatMemberToReturn;
        }

        public async Task<IEnumerable<ChatMemberDto>> GetActiveChatMembersByChatIdAsync(int chatId) {
            if (chatId < 1)
            {
                throw new InvalidParameterException($"Invalid chatId : {chatId}.");
            }
            string chatKey = $"chat:{chatId}:activeChatMembers";
            IEnumerable<ChatMember> chatMembers = await _cache.GetCachedDataAsync<IEnumerable<ChatMember>>(chatKey);
            if(!chatMembers.IsNullOrEmpty())
            {
                return _mapper.Map<IEnumerable<ChatMemberDto>>(chatMembers);
            }
            chatMembers = await _repository.ChatMember.GetActiveChatMembersByChatIdAsync(chatId);
            await _cache.SetCachedDataAsync(chatKey, chatMembers, TimeSpan.FromMinutes(30));
            return _mapper.Map<IEnumerable<ChatMemberDto>>(chatMembers);
        }

        public async Task<ChatMemberDto> GetChatMemberByChatIdUserIdAsync(int chatId, int userId)
        {
            if (userId < 1 || chatId < 1)
            {
                throw new InvalidParameterException($"Invalid chatId : {chatId} or userId : {userId}");
            }
            string chatMemberKey = $"chatMember:userId:{userId}:chatId:{chatId}";
            ChatMember? chatMember = await _cache.GetCachedDataAsync<ChatMember>(chatMemberKey);
            if(chatMember != null )
            {
                return _mapper.Map<ChatMemberDto>(chatMember);
            }

            chatMember = await _repository.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId) ?? throw new ChatMemberNotFoundException(chatId, userId); ;
            await _cache.SetCachedDataAsync(chatMemberKey, chatMember, TimeSpan.FromMinutes(30));
            return _mapper.Map<ChatMemberDto>(chatMember);
        }

        public async Task<bool> SetIsAdminAsync(int chatId, int userId, bool isAdmin)
        {
            if (userId < 1 || chatId < 1)
            {
                throw new InvalidParameterException($"Invalid chatId : {chatId} or userId : {userId}");
            }
            int affectedRows = await _repository.ChatMember.SetIsAdminAsync(chatId, userId, isAdmin);
            string chatMemberKey = $"chatMember:userId:{userId}:chatId:{chatId}";
            await _cache.RemoveDataAsync(chatMemberKey);
            await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
            return affectedRows > 0;
        }

        public async Task<bool> SetChatMemberStatus(int chatId, int userId, int statusId)
        {
            if (userId < 1 || chatId < 1 || statusId < 1 || statusId > 3)
            {
                throw new InvalidParameterException($"Invalid chatId : {chatId} or userId : {userId} or statusId: {statusId}");
            }
            int affectedRows = await _repository.ChatMember.SetChatMemberStatus(chatId, userId, statusId);
            await _cache.RemoveDataAsync($"chatMember:userId:{userId}:chatId:{chatId}");
            await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
            return affectedRows > 0;
        }

        public async Task<ChatMemberDto[]> InsertChatMembersAsync(int chatId, IEnumerable<int> userIds)
        {
            if(chatId < 1)
            {
                throw new InvalidParameterException($"Invalid chatId : {chatId}");
            }
            if(userIds.Any(i => i < 1))
            {
                throw new InvalidParameterException($"Invalid parameter user id list. An invalid member is detected.");
            }
            IEnumerable<ChatMember> chatMembers = await _repository.ChatMember.InsertChatMembers(chatId, userIds);
            await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
            return _mapper.Map<ChatMemberDto[]>(chatMembers);
        }

        public async Task<ChatMemberDto> InsertChatMemberAsync(int chatId, int userId)
        {
            if (chatId < 1 || userId < 1)
                throw new InvalidParameterException($"Invalid chat id : {chatId} and user id : {userId}.");

            User? user = await _repository.User.GetUserByIdAsync(userId) ?? throw new UserIdNotFoundException(userId);
            ChatMember? member = await _repository.ChatMember.GetChatMemberByChatIdAndUserIdAsync(chatId, userId);

            if (member == null)
            {
                IEnumerable<int> userIds = [userId];
                await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
                IEnumerable<ChatMember> chatMembers = await _repository.ChatMember.InsertChatMembers(chatId, userIds);
                return _mapper.Map<ChatMemberDto>(chatMembers.First());
            }

            if (await _repository.ChatMember.SetChatMemberStatus(chatId, userId, 1) < 1)
            {
                throw new ChatMemberNotUpdatedException(chatId, userId);
            }
            
            await _cache.RemoveDataAsync($"chat:{chatId}:activeChatMembers");
            member.StatusId = 1;
            ChatMemberDto  chatMemberDto = _mapper.Map<ChatMemberDto>(member);
            chatMemberDto.User =  _mapper.Map<UserDisplayDto>(user);
            return chatMemberDto;
        }
    }
}
