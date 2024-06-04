using Shared.DataTransferObjects.Messages;
using Shared.RequestFeatures;

namespace Service.Contracts {
    public interface IMessageService {
        Task<(IEnumerable<MessageDto> messages, MetaData? metaData)> GetMessagesByChatIdAsync(MessageParameters messageParameters, int chatId);
    }
}
