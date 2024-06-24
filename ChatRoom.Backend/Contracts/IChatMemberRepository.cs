using Entities.Models;
using Shared.Enums;

namespace Contracts {
    public interface IChatMemberRepository {
        Task<ChatMember?> UpdateLastSeenMessageAsync(ChatMember chatMember);
        Task<ChatMember?> GetChatMemberByChatIdAndUserIdAsync(int chatId, int userId);
        Task<IEnumerable<ChatMember>> InsertChatMembers(int chatId, IEnumerable<int> userIds);
        Task<IEnumerable<ChatMember>> GetActiveChatMembersByChatIdAsync(int chatId);
        // Task<ChatMember?> GetChatMemberByChatIdUserIdAsync(int chatId, int userId);
        Task<int> SetIsAdminAsync(int chatId, int userId, bool isAdmin);
        Task<int> SetChatMemberStatus(int chatId, int userId, int statusId);
    }
}
