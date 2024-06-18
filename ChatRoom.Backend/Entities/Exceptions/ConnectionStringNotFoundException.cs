using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class ConnectionStringNotFoundException(string connectionName): NotFoundException($"The connection string with name {connectionName} does not exists.") {
    }
}
