using Contracts;
using Dapper;
using Entities.Models;
using Microsoft.IdentityModel.Tokens;
using Shared.RequestFeatures;
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

        public async Task<IEnumerable<Chat>> GetChatListByUserIdAsync(ChatParameters chatParameters)
        {
            DynamicParameters parameters = new();
            parameters.Add("PageSize", chatParameters.PageSize);
            parameters.Add("PageNumber", chatParameters.PageNumber);
            parameters.Add("UserId", chatParameters.UserId);

            return await _connection.QueryAsync<Chat>("spGetChatListByUserId", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Chat>> SearchChatlistAsync(ChatParameters chatParameters)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", chatParameters?.UserId);
            parameters.Add("Name", String.IsNullOrEmpty(chatParameters?.Name) ? "" : chatParameters?.Name);
            parameters.Add("PageSize", chatParameters?.PageSize);
            parameters.Add("PageNumber", chatParameters?.PageNumber);

            return await _connection.QueryAsync<Chat>("spSearchChatlist", parameters, commandType : CommandType.StoredProcedure);
        }
    }
}
