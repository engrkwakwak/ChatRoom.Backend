using Entities.Models;
using Shared.RequestFeatures;

namespace Contracts {
    public interface IMessageRepository {
        Task<PagedList<Message>> GetMessagesByChatIdAsync(MessageParameters teacherParameters, int chatId);
        Task<Message?> InsertMessageAsync(Message message);
    }
}
