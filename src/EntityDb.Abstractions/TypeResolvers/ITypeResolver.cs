using System;
using System.Collections.Generic;

namespace EntityDb.Abstractions.TypeResolvers
{
    /// <summary>
    ///     Represents a type that resolves a <see cref="Type" />.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        ///     Returns the resolved <see cref="Type" /> or throws if the <see cref="Type" /> cannot be resolved.
        /// </summary>
        /// <param name="headers">Describes the type that needs to be resolved.</param>
        /// <returns>The resolved <see cref="Type" />.</returns>
        Type ResolveType(IReadOnlyDictionary<string, string> headers);
    }
}
