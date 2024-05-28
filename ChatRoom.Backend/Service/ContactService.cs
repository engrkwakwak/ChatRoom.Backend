using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Contacts;

namespace Service {
    internal sealed class ContactService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IContactService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<ContactDto> GetContactByUserIdContactId(int userId, int contactId)
        {
            Contact contact = await _repository.Contact.GetContactByUserIdContactIdAsync(userId, contactId);
            return _mapper.Map<ContactDto>(contact);
        }
    }
}
