using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository {
    public class UserRepository(IDbConnection connection) : IUserRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<User?> GetUserByUsernameAsync(string username) {
            DynamicParameters parameters = new();
            parameters.Add("username", username);

            User? user = await _connection.QuerySingleOrDefaultAsync<User>("spGetUserByUsername", parameters, commandType: CommandType.StoredProcedure);
            return user;
        }
        public async Task<User?> GetUserByEmailAsync(string email) {
            DynamicParameters parameters = new();
            parameters.Add("email", email);

            User? user = await _connection.QuerySingleOrDefaultAsync<User>("spGetUserByEmail", parameters, commandType: CommandType.StoredProcedure);
            return user;
        }

        public async Task<int> HasDuplicateEmail(string email) {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateEmail", new { Email = email }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> HasDuplicateUsername(string username) {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateUsername", new { Username = username }, commandType: CommandType.StoredProcedure);
        }

        public async Task<User> InsertUser(User user) {
            return await _connection.QueryFirstAsync<User>("spInsertUser", new {
                user.Username,
                user.DisplayName,
                user.Email,
                user.PasswordHash
            },
            commandType: CommandType.StoredProcedure);
        }
    }
}
