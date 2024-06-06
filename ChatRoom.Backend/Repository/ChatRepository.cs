using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository {
    public class ChatRepository(IDbConnection connection) : IChatRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<int> AddChatMembersAsync(int chatId, DataTable userIds)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserIds", userIds.AsTableValuedParameter());
            parameters.Add("ChatId", chatId);

            return await _connection.ExecuteAsync("spAddChatMembers", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Chat?> CreateChatAsync(int chatTypeId)
        {
            DynamicParameters parameters = new();
            parameters.Add("ChatTypeId", chatTypeId);
            return await _connection.QueryFirstOrDefaultAsync<Chat?>("spCreateChat", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Chat?> CreateChatAsync(Chat chatToCreate) {
            DynamicParameters parameters = new();
            parameters.Add("chatTypeId", chatToCreate.ChatTypeId);
            parameters.Add("statusId", chatToCreate.StatusId);

            Chat? createdChat = await _connection.QueryFirstOrDefaultAsync<Chat>("spCreateChat", parameters, commandType: CommandType.StoredProcedure);
            return createdChat;
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

        public async Task<IEnumerable<Chat>> GetChatsByUserIdAsync(int userId) {
            DynamicParameters parameters = new();
            parameters.Add("userId", userId);

            IEnumerable<Chat> chats = await _connection.QueryAsync<Chat>("spGetChatsByUserIdAsync", parameters, commandType: CommandType.StoredProcedure);
            return chats;
        }
    }
}
