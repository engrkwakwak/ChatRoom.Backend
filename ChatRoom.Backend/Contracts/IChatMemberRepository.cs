using Entities.Models;

namespace Contracts {
    public interface IChatMemberRepository {
        Task<IEnumerable<ChatMember>> InsertChatMembers(int chatId, IEnumerable<int> userIds);
    }
}
