using Contracts;
using System.Data;

namespace Repository {
    public class MessageRepository(IDbConnection connection) : IMessageRepository {
        private readonly IDbConnection _connection = connection;
    }
}
