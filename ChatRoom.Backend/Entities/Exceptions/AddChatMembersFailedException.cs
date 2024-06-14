using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class AddChatMembersFailedException(string message) : NoAffectedRowsException(message)
    {
    }
}
