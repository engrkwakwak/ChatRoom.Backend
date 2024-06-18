using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class ChatNotCreatedException(string chatMemberIds) : NoAffectedRowsException($"The server failed to create chat room for users: {chatMemberIds}. Try again later.")
    {
    }
}
