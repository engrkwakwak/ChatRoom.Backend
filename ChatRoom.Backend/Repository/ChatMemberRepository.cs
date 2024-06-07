using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository
{
    public class ChatMemberRepository(IDbConnection connection) : IChatMemberRepository
    {
        private readonly IDbConnection _connection = connection;

        public async Task<ChatMember?> GetChatMemberByChatIdUserIdAsync(int chatId, int userId)
        {
            DynamicParameters parameters = new();
            parameters.Add("ChatId", chatId);
            parameters.Add("UserId", userId);

            return await _connection.QueryFirstOrDefaultAsync<ChatMember>("spGetChatMemberByChatIdUserId", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
