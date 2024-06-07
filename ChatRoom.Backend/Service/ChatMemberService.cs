using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.ChatMembers;

namespace Service {
    internal sealed class ChatMemberService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IChatMemberService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<ChatMemberDto> GetChatMemberByChatIdUserIdAsync(int chatId, int userId)
        {
            ChatMember? chatMember = await _repository.ChatMember.GetChatMemberByChatIdUserIdAsync(chatId, userId);   
            if(chatMember == null)
            {
                throw new ChatMemberNotFoundException("Something went wrong while getting chat member. Please try again later.");
            }
            return _mapper.Map<ChatMemberDto>(chatMember);
        }
    }
}
