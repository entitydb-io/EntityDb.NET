using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;

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
    abstract static TProjection Construct(Id projectionId);

    /// <summary>
    ///     Returns the current version number of an entity.
    /// </summary>
    /// <returns></returns>
    VersionNumber GetEntityVersionNumber(Id entityId);

    /// <summary>
    ///     Returns a new <typeparamref name="TProjection"/> that incorporates the commands for a particular entity id.
    /// </summary>
    /// <param name="annotatedCommands">The annotated commands.</param>
    /// <returns>A new <typeparamref name="TProjection"/> that incorporates <paramref name="annotatedCommands"/>.</returns>
    TProjection Reduce(params IEntityAnnotation<object>[] annotatedCommands);
}
