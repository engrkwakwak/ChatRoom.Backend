using Entities.Models;

namespace Contracts {
    public interface IChatMemberRepository {
        Task<ChatMember?> GetChatMemberByChatIdUserIdAsync(int chatId, int userId);
    }
}
