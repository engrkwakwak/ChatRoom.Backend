using Contracts;
using Dapper;
using System.Data;

namespace Repository {
    public class UserRepository(IDbConnection connection) : IUserRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<int> HasDuplicateEmail(string email)
        {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateEmail", new { Email = email }, commandType: CommandType.StoredProcedure);

        }

        public async Task<int> HasDuplicateUsername(string username)
        {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateUsername", new { Username = username }, commandType: CommandType.StoredProcedure);
        }
    }
}
