using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public sealed class InvalidParameterException(string message) : BadRequestException(message)
    {
    }
}
