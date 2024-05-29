using Shared.DataTransferObjects.Contacts;
using Shared.RequestFeatures;

namespace Service.Contracts {
    public interface IContactService {
        public Task<ContactDto?> GetContactByUserIdContactIdAsync(int userId, int contactId);
        public Task<bool> InsertOrUpdateContactAsync(ContactForCreationDto contactForCreationDto);
        public Task<bool> DeleteContactByUserIdContactIdAsync(int userId, int contactId);
        public Task<IEnumerable<ContactDto>> GetContactsByUserIdAsync(ContactParameters contactParameters);
    }
}
