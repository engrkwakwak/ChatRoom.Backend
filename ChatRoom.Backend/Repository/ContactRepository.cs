using Contracts;
using Dapper;
using Entities.Models;
using Shared.RequestFeatures;
using System.Data;

namespace Repository {
    public class ContactRepository(IDbConnection connection) : IContactRepository {
        private readonly IDbConnection _connection = connection;

        public async Task<int> DeleteContactByUserIdContactIdAsync(int userId, int contactId)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", userId);
            parameters.Add("ContactId", contactId);

            return await _connection.ExecuteAsync("spDeleteContactByUserIdContactId", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Contact?> GetContactByUserIdContactIdAsync(int userId, int contactId)
        {
            DynamicParameters parameters = new();
            parameters.Add("user_id", userId);
            parameters.Add("contact_id", contactId);

            Contact? contact = await _connection.QuerySingleOrDefaultAsync<Contact>("spGetContactByUserIdContactId", parameters, commandType: CommandType.StoredProcedure);
            return contact;
        }

        public async Task<IEnumerable<Contact>> GetContactsByUserIdAsync(ContactParameters contactParameter)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", contactParameter.UserId);
            parameters.Add("PageSize", contactParameter.PageSize);
            parameters.Add("PageNumber", contactParameter.PageNumber);

            return await _connection.QueryAsync<Contact>("spGetContactsByUserId", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Contact?> InsertContactAsync(Contact contact)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", contact.UserId);
            parameters.Add("ContactId", contact.ContactId);
            parameters.Add("StatusId", contact.StatusId);

            Contact? _contact = await _connection.QuerySingleOrDefaultAsync<Contact>("spInsertContact", parameters, commandType: CommandType.StoredProcedure);
            return _contact;
        }

        public async Task<IEnumerable<User>> SearchContactsByNameUserIdAsync(ContactParameters contactParameter)
        {
            DynamicParameters parameters = new();
            parameters.Add("UserId", contactParameter.UserId);
            parameters.Add("PageSize", contactParameter.PageSize);
            parameters.Add("PageNumber", contactParameter.PageNumber);
            parameters.Add("Name", contactParameter.Name);

            return await _connection.QueryAsync<User>("spSearchContactsByNameUserId", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateContactStatusAsync(Contact contact)
        {
            DynamicParameters parameters = new();
            parameters.Add("user_id", contact.UserId);
            parameters.Add("contact_id", contact.ContactId);
            parameters.Add("status_id", contact.StatusId);

            int affectedRows = await _connection.ExecuteAsync("spUpdateContactStatus", parameters, commandType: CommandType.StoredProcedure);
            return affectedRows;
        }

        public async Task<IEnumerable<Contact>> InsertContactsAsync(int userId, List<int> contactIds) {
            DynamicParameters parameters = new();
            parameters.Add("userId", userId);
            parameters.Add("contactIds", ToUserIdDataTable(contactIds), DbType.Object);

            IEnumerable<Contact> contacts = await _connection.QueryAsync<Contact>("spInsertContacts", parameters, commandType: CommandType.StoredProcedure);
            return contacts;
        }

        private static DataTable ToUserIdDataTable(IEnumerable<int> userIds) {
            DataTable dt = new();
            dt.Columns.Add("UserId", typeof(int));

            foreach (int userId in userIds) {
                dt.Rows.Add(userId);
            }
            return dt;
        }
    }
}
