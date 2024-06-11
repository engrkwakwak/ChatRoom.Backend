using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.Extensions.Caching.Distributed;
using RedisCacheService;
using Service.Contracts;
using Shared.DataTransferObjects.Messages;
using Shared.RequestFeatures;
using System.Text.Json;

namespace Service {
    internal sealed class MessageService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IRedisCacheManager cache) : IMessageService {
        private readonly IRepositoryManager _repository = repository;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IRedisCacheManager _cache = cache;
        public async Task<MessageDto> InsertMessageAsync(MessageForCreationDto message)
        {
            Message _message = _mapper.Map<Message>(message);
            Message? _createdMessage = await _repository.Message.InsertMessageAsync(_message);
            if (_createdMessage == null)
            {
                throw new MessageNotCreatedException("Something went wrong while sending the message. Please try again later.");
            }
            Message? createdMessage = await _repository.Message.GetMessageByMessageIdAsync(_createdMessage!.MessageId);
            if (createdMessage == null)
            {
                throw new MessageNotCreatedException("Something went wrong while sending the message. Please try again later.");
            }


            _cache.SetCachedData<Message>($"message:{_createdMessage.MessageId}", _createdMessage, TimeSpan.FromMinutes(1));
            return _mapper.Map<MessageDto>(createdMessage);
        }

        public async Task<(IEnumerable<MessageDto> messages, MetaData? metaData)> GetMessagesByChatIdAsync(MessageParameters messageParameters, int chatId) {
            PagedList<Message> messagesWithMetaData = await _repository.Message.GetMessagesByChatIdAsync(messageParameters, chatId);
            IEnumerable<MessageDto> messagesDto = _mapper.Map<IEnumerable<MessageDto>>(messagesWithMetaData);
            return (messages: messagesDto, metaData: messagesWithMetaData.MetaData);
        }
    }
}
