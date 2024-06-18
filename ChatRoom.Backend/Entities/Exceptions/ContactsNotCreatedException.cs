using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class ContactsNotCreatedException(string message) : NoAffectedRowsException(message)
    {
    }
}
