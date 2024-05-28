﻿using Contracts;
using Dapper;
using Entities.Models;
using System.Data;
using System.Threading.Tasks;

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
        public async Task<User?> GetUserByIdAsync(int userId) {
            DynamicParameters parameters = new();
            parameters.Add("userId", userId);

            User? user = await _connection.QuerySingleOrDefaultAsync<User>("spGetUserById", parameters, commandType: CommandType.StoredProcedure);
            return user;
        }

        public async Task<int> HasDuplicateEmail(string email) {
            return await _connection.ExecuteScalarAsync<int>("spHasDuplicateEmail", new { Email = email }, commandType: CommandType.StoredProcedure);
        }
        public async Task<int> HasDuplicateUsername(string username) {
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

        public async Task<int> UpdateUserAsync(User user) {
            DynamicParameters parameters = new();
            parameters.Add("userId", user.UserId);
            parameters.Add("username", user.Username);
            parameters.Add("email", user.Email);
            parameters.Add("birthdate", user.BirthDate);
            parameters.Add("address", user.Address);
            parameters.Add("displayName", user.DisplayName);
            parameters.Add("isEmailVerified", user.IsEmailVerified);

            return await _connection.ExecuteAsync("spUpdateUser", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> VerifyEmailAsync(int id)
        {
            DynamicParameters parameters = new();
            parameters.Add("userid", id);
            return await _connection.ExecuteAsync("spVerifyEmail", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
