﻿namespace Entities.Exceptions.Base
{
    public abstract class BadRequestException(string message) : Exception(message)
    {
    }
}
