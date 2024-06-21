﻿using AutoMapper;
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

            string chatKey = $"chat:{chatId}:activeChatMembers";
            await _cache.RemoveDataAsync(chatKey);
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
    }
}
