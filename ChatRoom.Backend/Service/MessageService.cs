using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.Messages;
using Shared.RequestFeatures;

namespace Service; 
public sealed class MessageService(
    IRepositoryManager repository,
    IMapper mapper
) : IMessageService {

    private readonly IRepositoryManager _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<(IEnumerable<MessageDto> messages, MetaData? metaData)> GetMessagesByChatIdAsync(MessageParameters messageParameters, int chatId) {
        PagedList<Message> messagesWithMetaData = await _repository.Message.GetMessagesByChatIdAsync(messageParameters, chatId);
        IEnumerable<MessageDto> messagesDto = _mapper.Map<IEnumerable<MessageDto>>(messagesWithMetaData);
        return (messages: messagesDto, metaData: messagesWithMetaData.MetaData);
    }

    public async Task<MessageDto> InsertMessageAsync(MessageForCreationDto message) {
        Message messageEntity = _mapper.Map<Message>(message);
        Message createdMessage = await _repository.Message.InsertMessageAsync(messageEntity)
            ?? throw new MessageNotCreatedException();
        return _mapper.Map<MessageDto>(createdMessage);
    }

    public async Task<MessageDto> DeleteMessageAsync(int messageId)
    {
        Message messageToDelete = await GetMessageAndCheckIfItExists(messageId);

        int affectedRows = await _repository.Message.DeleteMessageAsync(messageId);
        if(affectedRows < 1) {
            throw new MessageUpdateFailedException("Something went wrong while deleting the message. Please try again later.");
        }

        MessageDto deletedMessage = _mapper.Map<MessageDto>(messageToDelete);
        return deletedMessage;
    }

    public async Task<MessageDto> GetMessageByMessageIdAsync(int messageId)
    {
        Message message = await GetMessageAndCheckIfItExists(messageId);
        MessageDto messageDto = _mapper.Map<MessageDto>(message);
        return messageDto;
    }

    public async Task<MessageDto> UpdateMessageAsync(MessageForUpdateDto message)
    {
        Message messageEntity = await GetMessageAndCheckIfItExists(message.MessageId);
        _mapper.Map(message, messageEntity);

        int affectedRows = await _repository.Message.UpdateMessageAsync(messageEntity);
        if(affectedRows < 1){
            throw new MessageUpdateFailedException("Something went wrong while updating the message. Please try again later.");
        }
        
        MessageDto updatedMessageDto = _mapper.Map<MessageDto>(messageEntity);
        return updatedMessageDto;
    }

    private async Task<Message> GetMessageAndCheckIfItExists(int messageId) {
        Message message = await _repository.Message.GetMessageByMessageIdAsync(messageId)
            ?? throw new MessageNotFoundException(messageId);
        return message;
    }
}
