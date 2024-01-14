﻿namespace EntityDb.Abstractions.States.Attributes;

/// <summary>
///     Represents a single metadata property and the context in which the metadata property must be unique.
/// </summary>
/// <remarks>
///     The source repository is responsible for enforcing the uniqueness constraint.
///     If a lease needs to be unique in a global context, a constant should be used as the <see cref="Scope" /> for all
///     instances of the lease.
///     If a lease does not need to be unique in a global context, the state id (or some other id which is unique to the
///     state) should be included in the <see cref="Scope" /> for all instances of the lease.
///     A lease may have additional properties, but they are not directly relevant to the uniqueness constraint.
/// </remarks>
public interface ILease
{
    /// <summary>
    ///     The context in which the metadata property must be unique.
    /// </summary>
    string Scope { get; }

    /// <summary>
    ///     The name of the metadata property.
    /// </summary>
    string Label { get; }

    /// <summary>
    ///     The value of the metadata property.
    /// </summary>
    string Value { get; }
}