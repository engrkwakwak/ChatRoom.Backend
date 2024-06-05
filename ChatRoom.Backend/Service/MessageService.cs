using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Messages;
using Shared.RequestFeatures;

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

        public async Task<(IEnumerable<MessageDto> messages, MetaData? metaData)> GetMessagesByChatIdAsync(MessageParameters messageParameters, int chatId) {
            PagedList<Message> messagesWithMetaData = await _repository.Message.GetMessagesByChatIdAsync(messageParameters, chatId);
            IEnumerable<MessageDto> messagesDto = _mapper.Map<IEnumerable<MessageDto>>(messagesWithMetaData);
            return (messages: messagesDto, metaData: messagesWithMetaData.MetaData);
        }
    }
}
