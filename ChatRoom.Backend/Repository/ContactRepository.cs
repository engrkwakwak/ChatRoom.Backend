using Contracts;
using Dapper;
using Entities.Models;
using Shared.RequestFeatures;
using System.Data;

namespace Repository {
    public class ContactRepository(IDbConnection connection) : IContactRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<int> DeleteContactByUserIdContactIdAsync(int id)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", id);

            return await _connection.ExecuteAsync("spDeleteContactByUserIdContactId", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Contact> GetContactByUserIdContactIdAsync(int userId, int contactId)
        {
            DynamicParameters parameters = new();
            parameters.Add("user_id", userId);
            parameters.Add("contact_id", contactId);

            return await _connection.QueryFirstAsync<Contact>("spGetContactByUserIdContactId", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Contact>> GetContactsByUserIdAsync(ContactParameters contactParameter)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", contactParameter.UserId);
            parameters.Add("PageSize", contactParameter.PageSize);
            parameters.Add("PageNumber", contactParameter.PageNumber);

            return await _connection.QueryAsync<Contact>("spGetContactsByUserId", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Contact> InsertContactAsync(Contact contact)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", contact.UserId);
            parameters.Add("ContactId", contact.ContactId);
            parameters.Add("StatusId", contact.StatusId);

            return await _connection.QueryFirstAsync<Contact>("spInsertContact", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
