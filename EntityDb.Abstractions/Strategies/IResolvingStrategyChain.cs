using System;
using System.Collections.Generic;

namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    /// Represents a type that resolves a <see cref="Type"/> by using a chain of <see cref="IResolvingStrategy"/>.
    /// </summary>
    public interface IResolvingStrategyChain
    {
        /// <summary>
        /// Returns the resolved <see cref="Type"/> or throws if the <see cref="Type"/> cannot be resolved.
        /// </summary>
        /// <param name="headers">Describes the type that needs to be resolved.</param>
        /// <returns>The resolved <see cref="Type"/>.</returns>
        Type ResolveType(IReadOnlyDictionary<string, string> headers);
    }
}
