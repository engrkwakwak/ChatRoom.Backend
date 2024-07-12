using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class NoChatMembersFoundException(int chatId) 
        : NotFoundException($"No members found for chat with chat id {chatId}.") {
    }
}
