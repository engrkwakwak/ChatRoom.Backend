using Entities.Models;
using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Users;

namespace Service.Contracts {
    public interface IChatService {
        Task<int?> GetP2PChatIdByUserIdsAsync(int userId1, int userId2);
        Task<ChatDto> CreateP2PChatAndAddMembersAsync(int userId1, int userId2);
        Task<ChatDto> GetChatByChatIdAsync(int chatId);
        Task<IEnumerable<UserDto>> GetActiveChatMembersByChatIdAsync(int chatId);
        Task<bool> DeleteChatAsync(int chatId);
        Task<bool> CanViewAsync(int chatId, int userId);

    }
}
