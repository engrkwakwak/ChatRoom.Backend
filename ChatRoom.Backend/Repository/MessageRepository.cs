using Contracts;
using Dapper;
using Entities.Models;
using Shared.RequestFeatures;
using System.Data;

namespace Repository {
    public class MessageRepository(IDbConnection connection) : IMessageRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<PagedList<Message>> GetMessagesByChatIdAsync(MessageParameters teacherParameters, int chatId) {
            DynamicParameters parameters = new();
            parameters.Add("chatId", chatId);
            parameters.Add("pageNumber", teacherParameters.PageNumber);
            parameters.Add("pageSize", teacherParameters.PageSize);

            IEnumerable<Message> messages = (await _connection.QueryAsync<Message, User, MessageType, Status, Message>(
                "spGetMessagesByChatId",
                (message, sender, msgType, status) => {
                    message.User = sender;
                    message.MessageType = msgType;
                    message.Status = status;
                    return message;
                },
                parameters,
                commandType: CommandType.StoredProcedure,
                splitOn: "UserId, MsgTypeId, StatusId"
                )).ToList();

            int count = await GetMessageCountByChatIdAsync(chatId);

            return new PagedList<Message>(messages.ToList(), count, teacherParameters.PageNumber, teacherParameters.PageSize);
        }

        private async Task<int> GetMessageCountByChatIdAsync(int chatId) {
            DynamicParameters parameters = new();
            parameters.Add("chatId", chatId);

            int count = await _connection.ExecuteScalarAsync<int>("spGetMessageCountByChatId", parameters, commandType: CommandType.StoredProcedure);
            return count;
        }
    }
}
