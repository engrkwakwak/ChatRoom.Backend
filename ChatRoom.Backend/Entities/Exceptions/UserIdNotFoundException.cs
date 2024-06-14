using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public sealed class UserIdNotFoundException(int userId) : NotFoundException($"The user with id: {userId} doesn't exists in the database.") {
    }
}
