using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Contacts;
using Shared.RequestFeatures;

namespace Service {
    internal sealed class ContactService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IContactService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> DeleteContactByUserIdContactId(int userId, int contactId)
        {
            return await _repository.Contact.DeleteContactByUserIdContactIdAsync(userId, contactId) > 0;
        }

        public async Task<ContactDto?> GetContactByUserIdContactId(int userId, int contactId)
        {
            Contact? contact = await _repository.Contact.GetContactByUserIdContactIdAsync(userId, contactId);
            if(contact == null)
            {
                return null;
            }
            return _mapper.Map<ContactDto>(contact);
        }

        public async Task<IEnumerable<ContactDto>> GetContactsByUserId(ContactParameters contactParameters)
        {
            IEnumerable<Contact> contacts = await _repository.Contact.GetContactsByUserIdAsync(contactParameters);
            IEnumerable<ContactDto> contactDtos = _mapper.Map<IEnumerable<ContactDto>>(contacts);
            return contactDtos;
        }

        public async Task<bool> InsertOrUpdateContact(ContactForCreationDto contactForCreationDto)
        {
            Contact contactToSave = _mapper.Map<Contact>(contactForCreationDto);
            Contact? contact = await _repository.Contact.GetContactByUserIdContactIdAsync((int)contactForCreationDto.UserId!, (int)contactForCreationDto.ContactId!);
            if(contact == null)
            {
                contact = await _repository.Contact.InsertContactAsync(contactToSave);
                return contact != null;
            }
            return await _repository.Contact.UpdateContactStatusAsync(contactToSave) > 0;

        }
    }
}
