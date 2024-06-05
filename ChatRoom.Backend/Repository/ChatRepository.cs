using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository {
    public class ChatRepository(IDbConnection connection) : IChatRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<int> AddP2PChatMembersAsync(int chatId, int userId1, int userId2)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId1", userId1);
            parameters.Add("UserId2", userId2);
            parameters.Add("ChatId", chatId);

            return await _connection.ExecuteAsync("spAddP2PChatMembers", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Chat?> CreateChatAsync(int chatTypeId)
        {
            DynamicParameters parameters = new();
            parameters.Add("ChatTypeId", chatTypeId);
            return await _connection.QueryFirstOrDefaultAsync<Chat?>("spCreateChat", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Chat?> GetChatByChatIdAsync(int chatId)
        {
            DynamicParameters parameters = new();
            parameters.Add("ChatId", chatId);

            return await _connection.QueryFirstOrDefaultAsync<Chat>("spGetChatByChatId", parameters, commandType : CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<User>> GetActiveChatMembersByChatIdAsync(int chatId)
        {
            DynamicParameters parameters = new();
            parameters.Add("ChatId", chatId);

            return await _connection.QueryAsync<User>("spGetChatMembers", parameters, commandType : CommandType.StoredProcedure);
        }

        public async Task<int?> GetP2PChatIdByUserIdsAsync(int userId1, int userId2)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId1", userId1);
            parameters.Add("UserId2", userId2);

            return await _connection.ExecuteScalarAsync<int?>("spGetPToPChatIdByUserIds", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
