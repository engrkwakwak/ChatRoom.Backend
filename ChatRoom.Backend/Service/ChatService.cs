using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using Shared.RequestFeatures;

namespace Service {
    internal sealed class ChatService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisCacheManager cache, IFileManager fileManager) : IChatService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IRedisCacheManager _cache = cache;
        private readonly IFileManager _fileManager = fileManager;

        public async Task<ChatDto> CreateChatWithMembersAsync(ChatForCreationDto chatDto) {
            Chat chatEntity = _mapper.Map<Chat>(chatDto);
            Chat? createdChat = await _repository.Chat.CreateChatAsync(chatEntity) ?? throw new ChatNotCreatedException($"{string.Join(",", chatDto.ChatMemberIds!)}");

            createdChat.Members = await _repository.ChatMember.InsertChatMembers(createdChat.ChatId, chatDto.ChatMemberIds!);
            if (createdChat.Members.Count() != chatDto.ChatMemberIds!.Count())
                throw new InsertedChatMemberRowsMismatchException(createdChat.Members.Count(), chatDto.ChatMemberIds!.Count());

            ChatDto chatToReturn = _mapper.Map<ChatDto>(createdChat);
            return chatToReturn;
        }

        public async Task<bool> CanViewAsync(int chatId, int userId)
        {
            IEnumerable<ChatMember> chatMembers = await _repository.ChatMember.GetActiveChatMembersByChatIdAsync(chatId);
            foreach (var member in chatMembers)
            {
                if(member.User?.UserId == userId)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<ChatDto> GetP2PChatByUserIdsAsync(int userId1, int userId2)
        {
            Chat? existingChat = await _repository.Chat.GetP2PChatByUserIdsAsync(userId1, userId2);
            ChatDto chatToReturn = _mapper.Map<ChatDto>(existingChat);
            return chatToReturn;
        }

        public async Task<ChatDto> GetChatByChatIdAsync(int chatId)
        {
            string chatCacheKey = $"chat:{chatId}";
            Chat? chat = await _cache.GetCachedDataAsync<Chat>(chatCacheKey);
            if (chat != null)
                return _mapper.Map<ChatDto>(chat);

            chat = await _repository.Chat.GetChatByChatIdAsync(chatId) ?? throw new ChatNotFoundException(chatId);
            _cache.SetCachedData(chatCacheKey, chat, TimeSpan.FromMinutes(30));
            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<bool> DeleteChatAsync(int chatId)
        {
            int affectedRows =  await _repository.Chat.DeleteChatAsync(chatId);
            if (affectedRows < 1)
                return false;

            string chatKey = $"chat:{chatId}";
            await _cache.RemoveDataAsync(chatKey);
            return true;
        }

        
        public async Task<IEnumerable<ChatDto>> GetChatListByChatIdAsync(ChatParameters chatParameters)
        {
            IEnumerable<Chat> chats = await _repository.Chat.SearchChatlistAsync(chatParameters);
            return _mapper.Map<IEnumerable<ChatDto>>(chats);
        }

        public async Task<IEnumerable<ChatDto>> GetChatsByUserIdAsync(int userId)
        {
            IEnumerable<Chat> userChats = await _repository.Chat.GetChatsByUserIdAsync(userId);
            IEnumerable<ChatDto> userChatsToReturn = _mapper.Map<IEnumerable<ChatDto>>(userChats);

            return userChatsToReturn;
        }

        public async Task UpdateChatAsync(int chatId, ChatForUpdateDto chat) {
            string cacheKey = $"chat:{chatId}";
            Chat chatEntity = await _repository.Chat.GetChatByChatIdAsync(chatId) ?? throw new ChatNotFoundException(chatId);

            if (!string.IsNullOrEmpty(chatEntity.DisplayPictureUrl) && !string.IsNullOrEmpty(chat.DisplayPictureUrl)) {
                await _fileManager.DeleteImageAsync(chatEntity.DisplayPictureUrl);
            }

            _mapper.Map(chat, chatEntity);

            int rowsAffected = await _repository.Chat.UpdateChatAsync(chatEntity);
            if (rowsAffected < 1) {
                _logger.LogError($"Failed to update the chat with id {chatEntity.ChatId}. " +
                    $"Total rows affected: {rowsAffected}. At {nameof(ChatService)} - {nameof(UpdateChatAsync)}.");
                throw new ChatUpdateFailedException(chatId);
            }

            await _cache.RemoveDataAsync(cacheKey);
        }
    }
}