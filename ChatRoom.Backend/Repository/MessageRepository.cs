using Contracts;
using Dapper;
using Entities.Models;
using Shared.RequestFeatures;
using System.Data;

namespace Repository {
    public class MessageRepository(IDbConnection connection) : IMessageRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<PagedList<Message>> GetMessagesByChatIdAsync(MessageParameters messageParameters, int chatId) {
            DynamicParameters parameters = new();
            parameters.Add("chatId", chatId);
            parameters.Add("pageNumber", messageParameters.PageNumber);
            parameters.Add("pageSize", messageParameters.PageSize);

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

            return new PagedList<Message>(messages.ToList(), count, messageParameters.PageNumber, messageParameters.PageSize);
        }

        private async Task<int> GetMessageCountByChatIdAsync(int chatId) {
            DynamicParameters parameters = new();
            parameters.Add("chatId", chatId);

            int count = await _connection.ExecuteScalarAsync<int>("spGetMessageCountByChatId", parameters, commandType: CommandType.StoredProcedure);
            return count;
        }

        public async Task<Message?> InsertMessageAsync(Message message)
        {
            DynamicParameters parameters = new();
            parameters.Add("ChatId", message.ChatId);
            parameters.Add("SenderId", message.SenderId);
            parameters.Add("Content", message.Content);
            parameters.Add("MessageTypeId", message.MsgTypeId);

            IEnumerable<Message> createdMessage =  await _connection.QueryAsync<Message, User, MessageType, Status, Message>(
                "spInsertMessage", 
                (message, sender, msgType, status) => {
                    message.User = sender;
                    message.MessageType = msgType;
                    message.Status = status;
                    return message;
                }, 
                parameters, 
                commandType: CommandType.StoredProcedure,
                splitOn: "UserId, MsgTypeId, StatusId"
            );
            return createdMessage.FirstOrDefault();
        }

        public async Task<Message?> GetMessageByMessageIdAsync(int messageId)
        {
            DynamicParameters parameters = new();
            parameters.Add("MessageId", messageId);

            Message? message = (await _connection.QueryAsync<Message, User, MessageType, Status, Message>(
                "spGetMessageByMessageId",
                (message, sender, msgType, status) =>
                {
                    message.User = sender;
                    message.MessageType = msgType;
                    message.Status = status;
                    return message;
                },
                parameters,
                commandType: CommandType.StoredProcedure,
                splitOn: "UserId, MsgTypeId, StatusId"
                ))
                .ToList()
                .FirstOrDefault();
            return message;
        }
    }
}
