using AutoMapper;
using Contracts;
using Entities.Models;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace Service {
    internal sealed class ContactService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IContactService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> DeleteContactByUserIdContactIdAsync(int userId, int contactId)
        {
            return await _repository.Contact.DeleteContactByUserIdContactIdAsync(userId, contactId) > 0;
        }

        public async Task<ContactDto?> GetContactByUserIdContactIdAsync(int userId, int contactId)
        {
            Contact? contact = await _repository.Contact.GetContactByUserIdContactIdAsync(userId, contactId);
            if(contact == null)
            {
                return null;
            }
            return _mapper.Map<ContactDto>(contact);
        }

        public async Task<IEnumerable<ContactDto>> GetContactsByUserIdAsync(ContactParameters contactParameters)
        {
            IEnumerable<Contact> contacts = await _repository.Contact.GetContactsByUserIdAsync(contactParameters);
            IEnumerable<ContactDto> contactDtos = _mapper.Map<IEnumerable<ContactDto>>(contacts);
            return contactDtos;
        }

        public async Task<bool> InsertOrUpdateContactAsync(ContactForCreationDto contactForCreationDto)
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

        public async Task<IEnumerable<UserDto>> SearchContactsByNameUserIdAsync(ContactParameters contactParameters)
        {
            IEnumerable<User> users;
            IEnumerable<UserDto> userDtos;
           if (!contactParameters.Name.IsNullOrEmpty())
            {
                users = await _repository.Contact.SearchContactsByNameUserIdAsync(contactParameters);
                userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
                return userDtos;
            }
            IEnumerable<Contact> contacts = await _repository.Contact.GetContactsByUserIdAsync(contactParameters);
            List<int> userIds = [];
            foreach (var contact in contacts)
            {
                userIds.Add(contact.ContactId);
            }
            string _userIds = string.Join(",", userIds);
            users = await _repository.User.GetUsersByIdsAsync(_userIds);
            userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return userDtos;
        }
    }
}
