using Entities.Models;

namespace Contracts {
    public interface IChatMemberRepository {
        Task<ChatMember?> UpdateLastSeenMessageAsync(ChatMember chatMember);
        Task<ChatMember?> GetChatMemberByChatIdAndUserIdAsync(int chatId, int userId);
        Task<IEnumerable<ChatMember>> InsertChatMembers(int chatId, IEnumerable<int> userIds);
        Task<IEnumerable<ChatMember>> GetActiveChatMembersByChatIdAsync(int chatId);
        // Task<ChatMember?> GetChatMemberByChatIdUserIdAsync(int chatId, int userId);
    }
}
