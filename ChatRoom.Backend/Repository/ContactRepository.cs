using Contracts;
using System.Data;

namespace Repository {
    public class ContactRepository(IDbConnection connection) : IContactRepository {
        private readonly IDbConnection _connection = connection;
    }
}
