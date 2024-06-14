namespace Entities.Exceptions.Base
{
    public abstract class NoAffectedRowsException(string message) : Exception(message)
    {
    }
}
