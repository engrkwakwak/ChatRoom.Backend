using Contracts;
using Dapper;
using Entities.Models;
using System.ComponentModel;
using System.Data;

namespace Repository {
    public class UserRepository(IDbConnection connection) : IUserRepository
    {
        private readonly IDbConnection _connection = connection;

        public async Task<int> HasDuplicateEmail(string email)
        {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateEmail", new { Email = email }, commandType: CommandType.StoredProcedure);

        }

        public async Task<int> HasDuplicateUsername(string username)
        {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateUsername", new { Username = username }, commandType: CommandType.StoredProcedure);
        }

        public async Task<User> InsertUser(User user)
        {
            return await _connection.QueryFirstAsync<User>("spInsertUser", new
            {
                user.Username,
                user.DisplayName,
                user.Email,
                user.PasswordHash
            },
            commandType: CommandType.StoredProcedure);
        }
    }
}
