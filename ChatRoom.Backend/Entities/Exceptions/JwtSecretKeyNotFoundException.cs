using Entities.Exceptions.Base;

namespace Entities.Exceptions {
    public class JwtSecretKeyNotFoundException() : NotFoundException("The secret key for jwt token is undefined.") {
    }
}
