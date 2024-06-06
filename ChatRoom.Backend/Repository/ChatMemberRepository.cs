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
            parameters.Add("userIds", ToIntDataTable(userIds), DbType.Object);

            IEnumerable<ChatMember> members = await _connection.QueryAsync<ChatMember>("spInsertChatMembers", parameters, commandType: CommandType.StoredProcedure);
            return members;
        }

        private static DataTable ToIntDataTable(IEnumerable<int> userIds) {
            DataTable dt = new();
            dt.Columns.Add("UserId", typeof(int));

            foreach(int userId in userIds) {
                dt.Rows.Add(userId);
            }
            return dt;
        }
    }
}
