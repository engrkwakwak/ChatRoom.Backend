using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class MessageNotFoundException(string message ) : NotFoundException(message)
    {
    }
}
