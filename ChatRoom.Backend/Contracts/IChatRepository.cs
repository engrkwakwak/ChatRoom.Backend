using Entities.Models;

namespace Contracts {
    public interface IChatRepository {
        
        ///<summary>
        ///<para>Gets the active ChatId of a P2P Chat based on the two UserIds.</para>
        ///<para>This can also be used to check if the two users has a current active chat</para>
        ///</summary>
        Task<int?> GetP2PChatIdByUserIdsAsync(int userId1, int userId2);
        Task<Chat?> CreateChatAsync(int chatTypeId);
        Task<int> AddP2PChatMembersAsync(int chatId, int userId1, int userId2);
        Task<Chat?> GetChatByChatIdAsync(int chatId);
        Task<IEnumerable<User>> GetActiveChatMembersByChatIdAsync(int chatId);
    }
}
