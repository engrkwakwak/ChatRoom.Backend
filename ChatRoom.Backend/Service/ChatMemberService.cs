using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;

namespace Service {
    internal sealed class ChatMemberService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IChatMemberService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

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
            IEnumerable<ChatMember> chatMembers = await _repository.ChatMember.GetActiveChatMembersByChatIdAsync(chatId);
            IEnumerable<ChatMemberDto> chatMembersToReturn = _mapper.Map<IEnumerable<ChatMemberDto>>(chatMembers);
            return chatMembersToReturn;
        }
    }
}
