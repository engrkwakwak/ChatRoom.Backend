using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class MessageUpdateFailedException(string message) : NoAffectedRowsException(message)
    {
    }
}
