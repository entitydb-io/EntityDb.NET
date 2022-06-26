using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Projections;

/// <summary>
///     Provides basic functionality for the common implementations.
/// </summary>
/// <typeparam name="TProjection"></typeparam>
public interface IProjection<TProjection> : ISnapshot<TProjection>
{
    /// <summary>
    ///     Returns a new <typeparamref name="TProjection"/> that incorporates the commands for a particular entity id.
    /// </summary>
    /// <param name="annotatedCommands">The annotated commands.</param>
    /// <returns>A new <typeparamref name="TProjection"/> that incorporates <paramref name="annotatedCommands"/>.</returns>
    TProjection Reduce(params IEntityAnnotation<object>[] annotatedCommands);

    /// <summary>
    ///     Returns the current version number of an entity.
    /// </summary>
    /// <returns></returns>
    VersionNumber GetEntityVersionNumber(Id entityId);
}
