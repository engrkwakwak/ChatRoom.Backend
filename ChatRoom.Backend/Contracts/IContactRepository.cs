using Entities.Models;
using Shared.RequestFeatures;

namespace Contracts {
    public interface IContactRepository {
        public Task<Contact> InsertContactAsync(Contact contact);
        public Task<IEnumerable<Contact>> GetContactsByUserIdAsync(ContactParameters contactParameter);
        public Task<int> DeleteContactByUserIdContactIdAsync(int id);
    }
}
