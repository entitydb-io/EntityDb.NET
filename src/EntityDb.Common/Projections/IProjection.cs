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
    ///     Returns a new <typeparamref name="TProjection" /> that incorporates the command for a particular entity id.
    /// </summary>
    /// <param name="annotatedCommand">The annotated command.</param>
    /// <returns>A new <typeparamref name="TProjection" /> that incorporates <paramref name="annotatedCommand" />.</returns>
    TProjection Reduce(IEntityAnnotation<object> annotatedCommand);

    /// <summary>
    ///     Returns a <see cref="ICommandQuery" /> that is used to load the rest of the state for the given projection pointer.
    /// </summary>
    /// <param name="projectionPointer">A pointer to the projection.</param>
    /// <returns>A <see cref="ICommandQuery" /> that is used to load the rest of the state for the given projection pointer.</returns>
    ICommandQuery GetCommandQuery(Pointer projectionPointer);

    /// <summary>
    ///     Maps an entity to a projection id, or default if the entity does not map to this projection.
    /// </summary>
    /// <param name="entity">The entity object.</param>
    /// <returns>The projection id for the entity, or default if none.</returns>
    static abstract Id? GetProjectionIdOrDefault(object entity);
}
