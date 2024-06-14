using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public sealed class UsernameNotFoundException(string username) : NotFoundException($"The user with username: {username} doesn't exists in the database.") {
    }
}
