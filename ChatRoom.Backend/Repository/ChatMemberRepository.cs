using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository
{
    public class ChatMemberRepository(IDbConnection connection) : IChatMemberRepository
    {
        private readonly IDbConnection _connection = connection;

        public async Task<IEnumerable<ChatMember>> InsertChatMembers(int chatId, IEnumerable<int> userIds) {
            DynamicParameters parameters = new();
            parameters.Add("chatId", chatId);
            parameters.Add("userIds", ToUserIdDataTable(userIds), DbType.Object);

            IEnumerable<ChatMember> members = await _connection.QueryAsync<ChatMember, User, ChatMember>(
                "spInsertChatMembers",
                (chatMember, user) => {
                    chatMember.User = user;
                    return chatMember;
                }, 
                parameters, 
                commandType: CommandType.StoredProcedure,
                splitOn: "UserId");
            return members;
        }

        public async Task<IEnumerable<ChatMember>> GetActiveChatMembersByChatIdAsync(int chatId) {
            DynamicParameters parameters = new();
            parameters.Add("chatId",chatId);

            IEnumerable<ChatMember> members = await _connection.QueryAsync<ChatMember, User, ChatMember>(
                "spGetChatMembers",
                (message, user) => {
                    message.User = user;
                    return message;
                },
                parameters, 
                commandType: CommandType.StoredProcedure,
                splitOn: "UserId"
            );
            return members;
        }

        public async Task<ChatMember?> GetChatMemberByChatIdAndUserIdAsync(int chatId, int userId) {
            DynamicParameters parameters = new();
            parameters.Add("chatId", chatId);
            parameters.Add("userId", userId);

            ChatMember? chatMember = await _connection.QuerySingleOrDefaultAsync<ChatMember>("spGetChatMemberByChatIdAndUserId", parameters, commandType: CommandType.StoredProcedure);
            return chatMember;
        }

        public async Task<ChatMember?> UpdateLastSeenMessageAsync(ChatMember chatMember) {
            DynamicParameters parameters = new();
            parameters.Add("chatId", chatMember.ChatId);
            parameters.Add("userId", chatMember.UserId);
            parameters.Add("lastSeenMessageId", chatMember.LastSeenMessageId);

            ChatMember? updatedChatMember = (await _connection.QueryAsync<ChatMember, User, ChatMember>(
                "spUpdateLastSeenMessage",
                (chatMember, user) => {
                    chatMember.User = user;
                    return chatMember;
                },
                parameters,
                commandType: CommandType.StoredProcedure,
                splitOn: "UserId"
            )).FirstOrDefault();
            return updatedChatMember;
        }

        private static DataTable ToUserIdDataTable(IEnumerable<int> userIds) {
            DataTable dt = new();
            dt.Columns.Add("UserId", typeof(int));

            foreach(int userId in userIds) {
                dt.Rows.Add(userId);
            }
            return dt;
        }

        public async Task<int> SetIsAdminAsync(int chatId, int userId, bool isAdmin)
        {
            DynamicParameters parameters = new();
            parameters.Add("ChatId", chatId);
            parameters.Add("UserId", userId);
            parameters.Add("IsAdmin", isAdmin);

            return await _connection.ExecuteScalarAsync<int>("spSetAdmin", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
