using System;
using System.Collections.Generic;

namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    /// Represents a type that resolves a <see cref="Type"/> or returns null.
    /// </summary>
    public interface IResolvingStrategy
    {
        /// <summary>
        /// Returns the resolved <see cref="Type"/> or null if the <see cref="Type"/> cannot be resolved.
        /// </summary>
        /// <param name="headers">Describes the type that needs to be resolved.</param>
        /// <returns>The resolved <see cref="Type"/> or null if the <see cref="Type"/> cannot be resolved.</returns>
        Type? ResolveType(IReadOnlyDictionary<string, string> headers);
    }
}
