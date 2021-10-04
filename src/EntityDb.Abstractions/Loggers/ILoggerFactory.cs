using System;

namespace EntityDb.Abstractions.Loggers
{
    /// <summary>
    ///     Represents a type used to create a <see cref="ILogger" />.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        ///     Returns a new <see cref="ILogger" />.
        /// </summary>
        /// <param name="type">The type that will be using the logger.</param>
        /// <returns>A new <see cref="ILogger" />.</returns>
        ILogger CreateLogger(Type type);
    }
}
