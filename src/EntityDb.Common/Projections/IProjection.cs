using System;

namespace EntityDb.Common.Projections;

/// <summary>
///     Provides basic functionality for the common implementations.
/// </summary>
/// <typeparam name="TProjection"></typeparam>
public interface IProjection<out TProjection>
{
    /// <summary>
    ///     Creates a new instance of a <typeparamref name="TProjection" />.
    /// </summary>
    /// <param name="projectionId">The id of the entity.</param>
    /// <returns>A new instance of <typeparamref name="TProjection" />.</returns>
    abstract static TProjection Construct(Guid projectionId);

    /// <summary>
    ///     Returns the current version number of an entity.
    /// </summary>
    /// <returns></returns>
    ulong GetEntityVersionNumber(Guid entityId);

    /// <summary>
    ///     Returns a new <typeparamref name="TProjection"/> that incorporates the commands for a particular entity id.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <param name="commands">The commands.</param>
    /// <returns>A new <typeparamref name="TProjection"/> that incorporates <paramref name="commands"/> for <paramref name="entityId"/>.</returns>
    TProjection Reduce(Guid entityId, params object[] commands);
}
