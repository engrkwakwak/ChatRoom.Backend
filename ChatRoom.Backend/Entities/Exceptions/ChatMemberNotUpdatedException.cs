using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class ChatMemberNotUpdatedException(int chatId, int userId) : NoAffectedRowsException($"The chat member with chat_id {chatId} user_id {userId} failed to update.") {

    }
}
