using Shared.DataTransferObjects.Messages;

namespace Service.Contracts {
    public interface IMessageService {
        Task<MessageDto> InsertMessageAsync(MessageForCreationDto message);
    }
}
