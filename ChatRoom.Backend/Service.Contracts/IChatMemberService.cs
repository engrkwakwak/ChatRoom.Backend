using Shared.DataTransferObjects.Chats;

namespace Service.Contracts {
    public interface IChatMemberService {
        Task<ChatMemberDto> UpdateLastSeenMessageAsync(int chatId, int userId, ChatMemberForUpdateDto chatMemberForUpdate);
        Task<IEnumerable<ChatMemberDto>> GetActiveChatMembersByChatIdAsync(int chatId);

        Task<ChatMemberDto> GetChatMemberByChatIdUserIdAsync(int chatId, int userId);

        Task<bool> SetIsAdminAsync(int chatId, int userId, bool isAdmin);

        Task<bool> SetChatMemberStatus(int chatId, int userId, int statusId);

        Task<ChatMemberDto[]> InsertChatMembersAsync(int chatId, IEnumerable<int> memberUserId);

        Task<ChatMemberDto> InsertChatMemberAsync(int chatId, int userId);
    }
}
