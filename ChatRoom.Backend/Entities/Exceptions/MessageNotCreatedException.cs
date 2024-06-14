using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class MessageNotCreatedException(string message) : NoAffectedRowsException(message)
    {
    }
}
