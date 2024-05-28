using Shared.DataTransferObjects.Contacts;
using Shared.RequestFeatures;

namespace Service.Contracts {
    public interface IContactService {
        public Task<ContactDto?> GetContactByUserIdContactId(int userId, int contactId);
        public Task<bool> InsertOrUpdateContact(ContactForCreationDto contactForCreationDto);
        public Task<bool> DeleteContactByUserIdContactId(int userId, int contactId);
        public Task<IEnumerable<ContactDto>> GetContactsByUserId(ContactParameters contactParameters);
    }
}
