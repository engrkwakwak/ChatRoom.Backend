using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class ChatNotFoundException(int chatId) : NotFoundException($"The chat with id {chatId} does not exists in the database.")
    {
    }
}
