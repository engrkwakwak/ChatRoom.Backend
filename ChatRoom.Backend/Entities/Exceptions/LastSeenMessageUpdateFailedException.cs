using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class LastSeenMessageUpdateFailedException(int chatId, int userId): NoAffectedRowsException($"The server failed to update the last seen message of user with id {userId} in chat with id {chatId}.") {
    }
}
