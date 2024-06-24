using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class ChatUpdateFailedException(int chatId) : NoAffectedRowsException($"The server failed to update the chat with id {chatId}.") {
    }
}
