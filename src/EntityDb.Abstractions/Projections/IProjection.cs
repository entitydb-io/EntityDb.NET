using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Projections;

/// <summary>
///     Provides basic functionality for the common implementation of projections.
/// </summary>
/// <typeparam name="TProjection"></typeparam>
public interface IProjection<TProjection> : IState<TProjection>
{
    /// <summary>
    ///     Incorporates the source into the projection.
    /// </summary>
    /// <param name="source">The source of information</param>
    void Mutate(Source source);

    /// <summary>
    ///     Returns a <see cref="ISourceDataQuery" /> that finds sources that need to be passed to the reducer.
    /// </summary>
    /// <param name="serviceProvider">A service provider for fetching repositories.</param>
    /// <param name="projectionPointer">A pointer to the desired projection state</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>
    ///     A <see cref="IDataQuery" /> that is used to load the rest of the messages for the given projection
    ///     pointer.
    /// </returns>
    IAsyncEnumerable<Source> EnumerateSources(IServiceProvider serviceProvider, Pointer projectionPointer,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Maps a source to a set of relevant state ids. May be empty if none of the messages in the source
    ///     are relevant.
    /// </summary>
    /// <param name="source">A source</param>
    /// <returns>The state ids for the projections.</returns>
    static abstract IEnumerable<Id> EnumerateRelevantStateIds(Source source);
}
