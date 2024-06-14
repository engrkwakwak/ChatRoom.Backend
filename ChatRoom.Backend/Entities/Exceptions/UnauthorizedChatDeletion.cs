using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Exceptions.Base;

namespace Entities.Exceptions
{
    public class UnauthorizedChatDeletion(string message) : UnauthorizedException(message)
    {
    }
}
