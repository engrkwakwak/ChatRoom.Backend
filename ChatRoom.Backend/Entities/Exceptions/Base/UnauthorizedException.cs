using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions.Base
{
    public abstract class UnauthorizedException(string message) : Exception(message)
    {
    }
}
