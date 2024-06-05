using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Messages;

namespace Service {
    internal sealed class MessageService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) : IMessageService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;

        public async Task<MessageDto> InsertMessageAsync(MessageForCreationDto message)
        {
            Message _message = _mapper.Map<Message>(message);
            Message? createdMessage = await _repository.Message.InsertMessageAsync(_message);
            if(createdMessage == null)
            {
                throw new MessageNotCreatedException("Something went wrong while sending the message. Please try again later.");
            }
            return _mapper.Map<MessageDto>(createdMessage);
        }
    }
}
