using System;

namespace EntityDb.Redis.Exceptions
{
    /// <summary>
    ///     The exception that is thrown when a snapshot envelope cannot be serialized.
    /// </summary>
    public sealed class SerializeException : Exception
    {
    }
}
