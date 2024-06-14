using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class UserUpdateFailedException(int userId) : NoAffectedRowsException($"The server failed to update the user with id: {userId}.")  {
    }
}
