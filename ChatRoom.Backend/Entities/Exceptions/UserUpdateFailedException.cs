using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions {
    public class UserUpdateFailedException(int userId) : NoAffectedRowsException($"The server failed to update the user with id: {userId}.")  {
    }
}
