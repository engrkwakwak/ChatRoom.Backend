﻿using Entities.Exceptions.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
    public class ChatNotDeletedException(string message) : NoAffectedRowsException(message)
    {
    }
}
