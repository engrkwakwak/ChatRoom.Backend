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
            Message? _createdMessage = await _repository.Message.InsertMessageAsync(_message) ?? throw new MessageNotCreatedException("Something went wrong while sending the message. Please try again later.");
            Message? createdMessage = await _repository.Message.GetMessageByMessageIdAsync(_createdMessage!.MessageId) ?? throw new MessageNotCreatedException("Something went wrong while sending the message. Please try again later.");
            return _mapper.Map<MessageDto>(createdMessage);
        }

        public async Task<(IEnumerable<MessageDto> messages, MetaData? metaData)> GetMessagesByChatIdAsync(MessageParameters messageParameters, int chatId) {
            PagedList<Message> messagesWithMetaData = await _repository.Message.GetMessagesByChatIdAsync(messageParameters, chatId);
            IEnumerable<MessageDto> messagesDto = _mapper.Map<IEnumerable<MessageDto>>(messagesWithMetaData);
            return (messages: messagesDto, metaData: messagesWithMetaData.MetaData);
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            int affectedRows = await _repository.Message.DeleteMessageAsync(messageId);
            return affectedRows > 0;
        }

        public async Task<MessageDto> GetMessageByMessageIdAsync(int messageId)
        {
            Message? createdMessage = await _repository.Message.GetMessageByMessageIdAsync(messageId);
            return createdMessage == null
                ? throw new MessageNotCreatedException("Something went wrong while sending the message. Please try again later.")
                : _mapper.Map<MessageDto>(createdMessage);
        }

        public async Task<MessageDto> UpdateMessageAsync(MessageForUpdateDto message)
        {
            Message _message = new Message
            {
                MessageId = message.MessageId,
                Content = message.Content,
            };

            int affectedRows = await _repository.Message.UpdateMessageAsync(_message);
            if(affectedRows < 1){
                throw new MessageUpdateFailedException("Something went wrong while deleting the message. Please try again later.");
            }

            Message? updatedMessage = await _repository.Message.GetMessageByMessageIdAsync(_message.MessageId);
            return updatedMessage == null
                ? throw new MessageNotCreatedException("Something went wrong while sending the message. Please try again later.")
                : _mapper.Map<MessageDto>(updatedMessage);
        }
    }
}
