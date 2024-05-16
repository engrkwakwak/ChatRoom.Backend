using Contracts;
using System.Data;

namespace Repository
{
    public class ChatMemberRepository(IDbConnection connection) : IChatMemberRepository
    {
        private readonly IDbConnection _connection = connection;
    }
}
