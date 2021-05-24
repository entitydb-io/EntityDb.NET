using System;

namespace EntityDb.Redis.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a snapshot envelope cannot be deserialized.
    /// </summary>
    public sealed class DeserializeException : Exception
    {
    }
}
