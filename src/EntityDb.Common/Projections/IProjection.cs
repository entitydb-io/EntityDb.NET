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
    ///     Returns a projection with a greater entity version number than before.
    /// </summary>
    /// <param name="entityId">The id of an entity for which to skip a version number.</param>
    /// <param name="skipCount">The number of entity version numbers to skip.</param>
    /// <returns></returns>
    TProjection SkipEntityVersionNumbers(Guid entityId, ulong skipCount);
}
