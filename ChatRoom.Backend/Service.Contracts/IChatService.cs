﻿using Shared.DataTransferObjects.Chats;
using Shared.DataTransferObjects.Users;

namespace Service.Contracts {
    public interface IChatService {
        Task<ChatDto> GetP2PChatByUserIdsAsync(int userId1, int userId2);
        Task<ChatDto> CreateChatWithMembersAsync(ChatForCreationDto chatDto);
        Task<ChatDto> GetChatByChatIdAsync(int chatId);
        Task<IEnumerable<ChatDto>> GetChatsByUserIdAsync(int userId);
    }
}
