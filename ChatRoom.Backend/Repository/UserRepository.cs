using Contracts;
using Dapper;
using System.Data;

namespace Repository {
    public class UserRepository(IDbConnection connection) : IUserRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<int> HasDuplicateEmail(string email)
        {
            int result = await _connection.ExecuteScalarAsync<int>("spHasDuplicateEmail", new { Email = email }, commandType: CommandType.StoredProcedure);
            return result;

        }
    }
}
