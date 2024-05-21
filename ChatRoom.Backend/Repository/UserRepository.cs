using Contracts;
using Dapper;
using Entities.Models;
using System.Data;

namespace Repository {
    public class UserRepository(IDbConnection connection) : IUserRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<User?> GetUserByUsernameAsync(string username) {
            var parameters = new DynamicParameters();
            parameters.Add("username", username);

            User? user = await _connection.QuerySingleOrDefaultAsync("spGetUserByUsername", parameters, commandType: CommandType.StoredProcedure);
            return user;
        }
        public async Task<User?> GetUserByEmailAsync(string email) {
            DynamicParameters parameters = new();
            parameters.Add("email", email);

            User? user = await _connection.QuerySingleOrDefaultAsync("spGetUserByEmail", parameters, commandType: CommandType.StoredProcedure);
            return user;    
        }
    }
}
