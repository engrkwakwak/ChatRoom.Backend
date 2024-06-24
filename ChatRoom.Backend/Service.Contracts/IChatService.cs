using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace Service.Contracts {
    public interface IChatService {
        Task<ChatDto> GetP2PChatByUserIdsAsync(int userId1, int userId2);
        Task<ChatDto> CreateChatWithMembersAsync(ChatForCreationDto chatDto);
        Task<ChatDto> GetChatByChatIdAsync(int chatId);
        Task<IEnumerable<ChatDto>> GetChatsByUserIdAsync(int userId);
        Task<bool> DeleteChatAsync(int chatId);
        Task<bool> CanViewAsync(int chatId, int userId);
        Task<IEnumerable<ChatDto>> GetChatListByChatIdAsync(ChatParameters chatParameters);
        Task UpdateChatAsync(int chatId, ChatForUpdateDto chat);
    }
}
