using Shared.DataTransferObjects.ChatMembers;

namespace Service.Contracts {
    public interface IChatMemberService {
        Task<ChatMemberDto> GetChatMemberByChatIdUserIdAsync(int chatId, int userId);
    }
}
