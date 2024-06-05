using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository {
    public class MessageRepository(IDbConnection connection) : IMessageRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<Message?> InsertMessageAsync(Message message)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("ChatId", message.ChatId);
            parameters.Add("SenderId", message.SenderId);
            parameters.Add("Content", message.Content);
            parameters.Add("MessageTypeId", message.MsgTypeId);

            return await _connection.QueryFirstOrDefaultAsync<Message?>("spInsertMessage", parameters, commandType:  CommandType.StoredProcedure);
        }
    }
}
