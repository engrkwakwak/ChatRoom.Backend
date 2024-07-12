using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class MessageNotCreatedException() 
        : NoAffectedRowsException("Something went wrong while sending the message. Please try again later.")
    {
    }
}
