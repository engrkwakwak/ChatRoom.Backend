using Contracts;
using Dapper;
using Entities.Models;
using Shared.RequestFeatures;
using System.Data;

namespace Repository {
    public class ContactRepository(IDbConnection connection) : IContactRepository {
        private readonly IDbConnection _connection = connection;

        public Task<int> DeleteContactByUserIdContactIdAsync(int id)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", id);

            return _connection.ExecuteAsync("spDeleteContactByUserIdContactId", parameters, commandType: CommandType.StoredProcedure);
        }

        public Task<IEnumerable<Contact>> GetContactsByUserIdAsync(ContactParameters contactParameter)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", contactParameter.UserId);
            parameters.Add("PageSize", contactParameter.PageSize);
            parameters.Add("PageNumber", contactParameter.PageNumber);

            return _connection.QueryAsync<Contact>("spGetContactsByUserId", parameters, commandType: CommandType.StoredProcedure);
        }

        public Task<Contact> InsertContactAsync(Contact contact)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", contact.UserId);
            parameters.Add("ContactId", contact.ContactId);
            parameters.Add("StatusId", contact.StatusId);

            return _connection.QueryFirstAsync<Contact>("spInsertContact", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
