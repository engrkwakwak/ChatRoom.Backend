using Entities.Models;
using Shared.RequestFeatures;
using System.Data;

namespace Contracts {
    public interface IChatRepository {
        
        ///<summary>
        ///<para>Gets the active ChatId of a P2P Chat based on the two UserIds.</para>
        ///<para>This can also be used to check if the two users has a current active chat</para>
        ///</summary>
        Task<int?> GetP2PChatIdByUserIdsAsync(int userId1, int userId2);
        Task<Chat?> CreateChatAsync(int chatTypeId);
        Task<int> AddChatMembersAsync(int chatId, DataTable userIds);
        Task<Chat?> GetChatByChatIdAsync(int chatId);
        Task<IEnumerable<User>> GetActiveChatMembersByChatIdAsync(int chatId);
        Task<IEnumerable<Chat>> GetChatListByUserIdAsync(ChatParameters chatParameters);
        Task<IEnumerable<Chat>> SearchChatlistAsync(ChatParameters chatParameters);
    }
}
