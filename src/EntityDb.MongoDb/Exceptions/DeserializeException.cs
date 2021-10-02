using System;

namespace EntityDb.MongoDb.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an object envelope cannot be deserialized. Possible objects include: sources, commands, facts, and leases.
    /// </summary>
    public sealed class DeserializeException : Exception
    {
    }
}
