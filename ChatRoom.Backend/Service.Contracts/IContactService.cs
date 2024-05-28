using Shared.DataTransferObjects.Contacts;

namespace Service.Contracts {
    public interface IContactService {
        public Task<ContactDto> GetContactByUserIdContactId(int userId, int contactId);
    }
}
