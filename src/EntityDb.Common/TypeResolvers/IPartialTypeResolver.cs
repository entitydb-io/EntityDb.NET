using EntityDb.Common.Envelopes;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Common.TypeResolvers;

/// <summary>
///     Represents a type that resolves a <see cref="Type" /> or returns null.
/// </summary>
public interface IPartialTypeResolver
{
    /// <summary>
    ///     Returns the resolved <see cref="Type" /> or null if the <see cref="Type" /> cannot be resolved.
    /// </summary>
    /// <param name="envelopeHeaders">Describes the type that needs to be resolved.</param>
    /// <param name="resolvedType">The resolved <see cref="Type" /> or null if the <see cref="Type" /> cannot be resolved.</param>
    /// <returns>
    ///     <c>true</c> if <paramref name="resolvedType" /> is not null or <c>false</c> if
    ///     <paramref name="resolvedType" /> is null.
    /// </returns>
    bool TryResolveType(EnvelopeHeaders envelopeHeaders, [NotNullWhen(true)] out Type? resolvedType);
}
