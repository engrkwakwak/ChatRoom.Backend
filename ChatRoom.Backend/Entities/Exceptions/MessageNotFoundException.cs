using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class MessageNotFoundException(int messageId) 
        : NotFoundException($"The message with id {messageId} does not exist in the database.")
    {
    }
}
