using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace Service.Contracts {
    public interface IContactService {
        public Task<ContactDto?> GetContactByUserIdContactIdAsync(int userId, int contactId);
        public Task<bool> InsertOrUpdateContactAsync(ContactForCreationDto contactForCreationDto);
        public Task<bool> DeleteContactByUserIdContactIdAsync(int userId, int contactId);
        public Task<IEnumerable<ContactDto>> GetContactsByUserIdAsync(ContactParameters contactParameters);
        public Task<IEnumerable<UserDto>> SearchContactsByNameUserIdAsync(ContactParameters contactParameters);
        Task<IEnumerable<ContactDto>> InsertContactsAsync(int userId, List<int> contactIds);
    }
}
