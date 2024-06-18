using Entities.Models;
using Shared.RequestFeatures;

namespace Contracts {
    public interface IMessageRepository {
        Task<PagedList<Message>> GetMessagesByChatIdAsync(MessageParameters teacherParameters, int chatId);
        Task<Message?> GetMessageByMessageIdAsync(int messageId);
        Task<Message?> InsertMessageAsync(Message message);
        Task<int> DeleteMessageAsync(int messageId);
        Task<int> UpdateMessageAsync(Message message);

    }
}
