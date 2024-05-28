using Contracts;
using Dapper;
using Entities.Models;
using Shared.RequestFeatures;
using System.Data;

namespace Repository {
    public class UserRepository(IDbConnection connection) : IUserRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<User?> GetUserByIdAsync(int id) {
            DynamicParameters parameters = new();
            parameters.Add("userid", id);

            User? user = await _connection.QuerySingleOrDefaultAsync<User>("spGetUserById", parameters, commandType: CommandType.StoredProcedure);
            return user;
        }
        
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

        public async Task<int> HasDuplicateEmailAsync(string email) {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateEmail", new { Email = email }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> HasDuplicateUsernameAsync(string username) {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateUsername", new { Username = username }, commandType: CommandType.StoredProcedure);
        }

        public async Task<User> InsertUserAsync(User user) {
            return await _connection.QueryFirstAsync<User>("spInsertUser", new {
                user.Username,
                user.DisplayName,
                user.Email,
                user.PasswordHash
            },
            commandType: CommandType.StoredProcedure);
        }

        public async Task<int> VerifyEmailAsync(int id)
        {
            DynamicParameters parameters = new();
            parameters.Add("userid", id);
            return await _connection.ExecuteAsync("spVerifyEmail", parameters, commandType: CommandType.StoredProcedure);
        }

        public Task<IEnumerable<User>> SearchUsersByNameAsync(UserParameters userParameters)
        {
            DynamicParameters parameters = new();
            parameters.Add("PageSize", userParameters.PageSize);
            parameters.Add("PageNumber", userParameters.PageNumber);
            parameters.Add("Name", userParameters.Name);

            return _connection.QueryAsync<User>("spSearchUsersByName", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
