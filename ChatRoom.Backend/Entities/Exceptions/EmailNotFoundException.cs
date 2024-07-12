using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public sealed class EmailNotFoundException(string email) 
        : NotFoundException($"The user with email: {email} doesn't exists in the database.") {
    }
}
