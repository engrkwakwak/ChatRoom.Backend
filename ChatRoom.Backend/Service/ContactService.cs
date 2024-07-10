using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.IdentityModel.Tokens;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Contacts;
using Shared.DataTransferObjects.Users;
using Shared.RequestFeatures;

namespace Service {
    internal sealed class ContactService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisCacheManager cache) : IContactService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IRedisCacheManager _cache = cache;

        public async Task<bool> DeleteContactByUserIdContactIdAsync(int userId, int contactId)
        {
            if (userId < 1 || contactId < 1 || userId == contactId)
                throw new InvalidParameterException($"Invalid parameter user id {userId} and contact id {contactId}.");
            int affectedRows = await _repository.Contact.DeleteContactByUserIdContactIdAsync(userId, contactId);
            if(affectedRows < 1)
                return false;
            string contactCacheKey = $"contact:{contactId}:user:{userId}";
            string userContactsCacheKey = $"user:{userId}:contacts:page:1";
            await _cache.RemoveDataAsync(contactCacheKey);
            await _cache.RemoveDataAsync(userContactsCacheKey);
            return true;
        }

        public async Task<ContactDto?> GetContactByUserIdContactIdAsync(int userId, int contactId)
        {
            if (userId < 1 || contactId < 1 || userId == contactId)
                throw new InvalidParameterException($"Invalid parameter user id {userId} and contact id {contactId}.");
            string contactCacheKey = $"contact:{contactId}:user:{userId}";
            Contact? contact = await _cache.GetCachedDataAsync<Contact>(contactCacheKey);
            if(contact != null) 
                return _mapper.Map<ContactDto>(contact);

            contact = await _repository.Contact.GetContactByUserIdContactIdAsync(userId, contactId);
            await _cache.SetCachedDataAsync(contactCacheKey, contact, TimeSpan.FromMinutes(30));
            return contact == null ? null : _mapper.Map<ContactDto>(contact);
        }

        public async Task<IEnumerable<ContactDto>> GetContactsByUserIdAsync(ContactParameters contactParameters)
        {
            string userContactsCacheKey = $"user:{contactParameters.UserId}:contacts:page:1";
            IEnumerable<Contact> contacts = await _cache.GetCachedDataAsync<IEnumerable<Contact>>(userContactsCacheKey);
            if (!contacts.IsNullOrEmpty() && contactParameters.PageNumber == 1)
                return _mapper.Map<IEnumerable<ContactDto>>(contacts);

            contacts = await _repository.Contact.GetContactsByUserIdAsync(contactParameters);
            if(contactParameters.PageNumber == 1)
                await _cache.SetCachedDataAsync(userContactsCacheKey, contacts, TimeSpan.FromMinutes(30));
            return _mapper.Map<IEnumerable<ContactDto>>(contacts);
        }

        public async Task<bool> InsertOrUpdateContactAsync(ContactForCreationDto contactForCreationDto)
        {
            string userContactsCacheKey = $"user:{contactForCreationDto.UserId}:contacts:page:1";
            Contact contactToSave = _mapper.Map<Contact>(contactForCreationDto);
            Contact? existingContact = await _repository.Contact.GetContactByUserIdContactIdAsync((int)contactForCreationDto.UserId!, (int)contactForCreationDto.ContactId!);
            if(existingContact == null)
            {
                existingContact = await _repository.Contact.InsertContactAsync(contactToSave);
                await _cache.RemoveDataAsync(userContactsCacheKey);
                return existingContact != null;
            }

            int affectedRows = await _repository.Contact.UpdateContactStatusAsync(contactToSave);
            if (affectedRows < 1)
                return false;

            string contactCacheKey = $"contact:{existingContact.ContactId}:user:{contactForCreationDto.UserId}";
            await _cache.RemoveDataAsync(contactCacheKey);
            await _cache.RemoveDataAsync(userContactsCacheKey);
            return true;
        }

        public async Task<IEnumerable<UserDto>> SearchContactsByNameUserIdAsync(ContactParameters contactParameters)
        {
            string userContactsCacheKey = $"user:{contactParameters.UserId}:contacts:page:1";
            IEnumerable<User> users;
            IEnumerable<UserDto> userDtos;
           if (!contactParameters.Name.IsNullOrEmpty())
            {
                users = await _repository.Contact.SearchContactsByNameUserIdAsync(contactParameters);
                userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
                return userDtos;
            }
            users = await _cache.GetCachedDataAsync<IEnumerable<User>>(userContactsCacheKey);
            if(!users.IsNullOrEmpty() && contactParameters.PageNumber == 1)
            {
                return _mapper.Map<IEnumerable<UserDto>>(users);
            }
            IEnumerable<Contact> contacts = await _repository.Contact.GetContactsByUserIdAsync(contactParameters);
            users = await _repository.User.GetUsersByIdsAsync(string.Join(",", contacts.Select(s => s.ContactId)));
            if (contactParameters.PageNumber == 1)
            {
                await _cache.SetCachedDataAsync(userContactsCacheKey, users, TimeSpan.FromMinutes(30));
            }
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<IEnumerable<ContactDto>> InsertContactsAsync(int userId, List<int> contactIds) {
            if (userId < 1 || contactIds.IsNullOrEmpty() || contactIds.Count(c => c < 1) > 0)
            {
                throw new InvalidParameterException("The contact id's and user id provided is invalid.");
            }
            IEnumerable<Contact> chatContacts = await _repository.Contact.InsertContactsAsync(userId, contactIds);
            IEnumerable<ContactDto> chatContactsToReturn = _mapper.Map<IEnumerable<ContactDto>>(chatContacts);
            
            if(chatContactsToReturn.Count() != contactIds.Count) throw new InsertedContactRowsMismatchException(chatContactsToReturn.Count(), contactIds.Count);
            string userContactsCacheKey = $"user:{userId}:contacts:page:1";
            await _cache.RemoveDataAsync(userContactsCacheKey);
            return chatContactsToReturn;
        }
    }
}
