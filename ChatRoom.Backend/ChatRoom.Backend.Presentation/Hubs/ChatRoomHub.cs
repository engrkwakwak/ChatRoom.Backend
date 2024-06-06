using Contracts;
using Microsoft.AspNetCore.SignalR;
using Service.Contracts;
using System.Security.Claims;

namespace ChatRoom.Backend.Presentation.Hubs {
    public class ChatRoomHub(IServiceManager service, ILoggerManager logger) : Hub {
        private readonly IServiceManager _service = service;
        private readonly ILoggerManager _logger = logger;

        public override async Task OnConnectedAsync() {
            var userId = GetUserIdFromSession();
            if (userId == 0) {
                Context.Abort();
                return;
            }

            var userChats = await _service.ChatService.GetChatsByUserIdAsync(userId);
            foreach (var chat in userChats) {
                var groupName = GetGroupName(chat.ChatId);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception) {
            var userId = GetUserIdFromSession();
            if (userId != 0) {
                var userChats = await _service.ChatService.GetChatsByUserIdAsync(userId);
                foreach (var chat in userChats) {
                    var groupName = GetGroupName(chat.ChatId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                }
            }

            await base.OnDisconnectedAsync(exception);
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
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null) {
                _logger.LogError("The user id found in the jwt token is null.");
                return 0;
            }

            return int.Parse(userIdClaim);
        }

        private static string GetGroupName(int chatId) => $"chat-{chatId}";
    }
}
