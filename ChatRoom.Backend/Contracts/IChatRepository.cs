using Entities.Models;

namespace Contracts {
    public interface IChatRepository {
        Task<Chat?> CreateChatAsync(Chat chatToCreate);
        Task<Chat?> GetChatByChatIdAsync(int chatId);
        Task<Chat?> GetP2PChatByUserIdsAsync(int userId1, int userId2);

        Task<IEnumerable<Chat>> GetChatsByUserIdAsync(int userId);

        Task<int> DeleteChatAsync(int chatId);
    }
}
