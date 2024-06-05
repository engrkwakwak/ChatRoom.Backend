using Entities.Models;

namespace Contracts {
    public interface IMessageRepository {
        Task<Message?> InsertMessageAsync(Message message);
    }
}
