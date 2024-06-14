using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class ChatMemberNotFoundException(int chatId, int userId) : NotFoundException($"The chat with id {chatId} does not contain a member with id {userId}.") {

    }
}
