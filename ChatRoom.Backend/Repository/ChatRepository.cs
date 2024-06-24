using Contracts;
using Dapper;
using Entities.Models;
using Shared.RequestFeatures;
using System.Data;

namespace Repository {
    public class ChatRepository(IDbConnection connection) : IChatRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<Chat?> CreateChatAsync(Chat chatToCreate) {
            DynamicParameters parameters = new();
            parameters.Add("chatTypeId", chatToCreate.ChatTypeId);
            parameters.Add("chatName", chatToCreate.ChatName);
            parameters.Add("displayPictureUrl", chatToCreate.DisplayPictureUrl);

            Chat? createdChat = await _connection.QueryFirstOrDefaultAsync<Chat>("spCreateChat", parameters, commandType: CommandType.StoredProcedure);
            return createdChat;
        }

        public async Task<Chat?> GetChatByChatIdAsync(int chatId)
        {
            DynamicParameters parameters = new();
            parameters.Add("ChatId", chatId);

            return await _connection.QueryFirstOrDefaultAsync<Chat>("spGetChatByChatId", parameters, commandType : CommandType.StoredProcedure);
        }

        public async Task<Chat?> GetP2PChatByUserIdsAsync(int userId1, int userId2) {
            DynamicParameters parameters = new();
            parameters.Add("userId1", userId1);
            parameters.Add("userId2", userId2);

            Chat? chat =  await _connection.QuerySingleOrDefaultAsync<Chat>("spGetPToPChatByUserIds", parameters, commandType: CommandType.StoredProcedure);
            return chat;
        }

        public async Task<IEnumerable<Chat>> GetChatsByUserIdAsync(int userId) {
            DynamicParameters parameters = new();
            parameters.Add("userId", userId);

            IEnumerable<Chat> chats = await _connection.QueryAsync<Chat>("spGetChatsByUserId", parameters, commandType: CommandType.StoredProcedure);
            return chats;
        }

        public async Task<int> DeleteChatAsync(int chatId)
        {
            DynamicParameters parameter = new();
            parameter.Add("ChatId", chatId);

            return await _connection.ExecuteAsync("spDeleteChat", parameter, commandType: CommandType.StoredProcedure);
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
        public async Task<int> UpdateChatAsync(Chat chat) {
            DynamicParameters parameters = new();
            parameters.Add("ChatId", chat.ChatId);
            parameters.Add("ChatName", chat.ChatName);
            parameters.Add("DisplayPictureUrl", chat.DisplayPictureUrl);

            return await _connection.ExecuteAsync("spUpdateChat", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
