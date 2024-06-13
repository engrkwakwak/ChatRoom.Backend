using Shared.DataTransferObjects.Chats;

namespace Service.Contracts {
    public interface IChatMemberService {
        Task<ChatMemberDto> UpdateLastSeenMessageAsync(int chatId, int userId, ChatMemberForUpdateDto chatMemberForUpdate);
        Task<IEnumerable<ChatMemberDto>> GetActiveChatMembersByChatIdAsync(int chatId);

    }
}
