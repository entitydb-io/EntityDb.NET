using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Projections;

/// <summary>
///     Provides basic functionality for the common implementation of projections.
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
    ///     Returns a <see cref="ICommandQuery"/> that is used to load the rest of the state for the given projection pointer.
    /// </summary>
    /// <param name="projectionPointer">A pointer to the projection.</param>
    /// <returns>A <see cref="ICommandQuery"/> that is used to load the rest of the state for the given projection pointer.</returns>
    ICommandQuery GetCommandQuery(Pointer projectionPointer);

    /// <summary>
    ///     Maps an entity to a projection id, or default if the entity does not map to this projection.
    /// </summary>
    /// <param name="entity">The entity object.</param>
    /// <returns>The projection id for the entity, or default if none.</returns>
    abstract static Id? GetProjectionIdOrDefault(object entity);
}
