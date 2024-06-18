using Contracts;
using Microsoft.AspNetCore.SignalR;
using Service.Contracts;
using Shared.DataTransferObjects.Chats;
using System.Security.Claims;

namespace ChatRoom.Backend.Presentation.Hubs {
    public class ChatRoomHub(IServiceManager service, ILoggerManager logger) : Hub {
        private readonly IServiceManager _service = service;
        private readonly ILoggerManager _logger = logger;

        public override async Task OnConnectedAsync() {
            await RegisterUserGroups();
            await base.OnConnectedAsync();
        }

        public async Task RegisterUserGroups() {
            int userId = GetUserIdFromSession();
            if (userId == 0) {
                Context.Abort();
                return;
            }

            IEnumerable<ChatDto> userChats = await _service.ChatService.GetChatsByUserIdAsync(userId);
            foreach (ChatDto chat in userChats) {
                await AddToGroup(chat.ChatId);
            }
        }

        public async Task AddToGroup(int chatId) {
            var groupName = GetGroupName(chatId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroupAsync(int chatId) {
            var groupName = GetGroupName(chatId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        private int GetUserIdFromSession() {
            string? userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) {
                _logger.LogError("The user id found in the jwt token is null.");
                return 0;
            }
            return int.Parse(userIdClaim);
        }

        public static string GetGroupName(int chatId) => $"chat-{chatId}";
    }
}
