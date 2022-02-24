using System;

namespace EntityDb.Common.Entities;

/// <summary>
///     Provides basic functionality for the common implementations.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IEntity<out TEntity>
{
    /// <summary>
    ///     Creates a new instance of a <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <returns>A new instance of <typeparamref name="TEntity" />.</returns>
    abstract static TEntity Construct(Guid entityId);

    /// <summary>
    ///     Returns the version number of the entity.
    /// </summary>
    /// <returns></returns>
    ulong GetVersionNumber();
}
