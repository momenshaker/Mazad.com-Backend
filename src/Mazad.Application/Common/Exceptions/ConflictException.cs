using System;

namespace Mazad.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException()
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string entityName, object key)
        : base($"Conflict detected for {entityName} with key '{key}'.")
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
