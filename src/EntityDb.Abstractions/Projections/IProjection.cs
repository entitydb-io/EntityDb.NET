using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Projections;

/// <summary>
///     Provides basic functionality for the common implementation of projections.
/// </summary>
/// <typeparam name="TProjection"></typeparam>
public interface IProjection<TProjection> : ISnapshot<TProjection>
{
    /// <summary>
    ///     Incorporates the source into the projection.
    /// </summary>
    /// <param name="source">The source of information</param>
    void Mutate(Source source);

    /// <summary>
    ///     Returns a <see cref="IMessageGroupQuery" /> that finds sources that need to be passed to the reducer.
    /// </summary>
    /// <param name="serviceProvider">A service provider for fetching repositories.</param>
    /// <param name="projectionPointer">A pointer to the desired projection state</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>
    ///     A <see cref="IQuery" /> that is used to load the rest of the source messages for the given projection
    ///     pointer.
    /// </returns>
    IAsyncEnumerable<Source> EnumerateSources(IServiceProvider serviceProvider, Pointer projectionPointer,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Maps a source to a set of entity ids. May be empty if the source does not map to the projection.
    /// </summary>
    /// <param name="source">A source</param>
    /// <returns>The entity branches for the projections.</returns>
    static abstract IEnumerable<Id> EnumerateEntityIds(Source source);
}
