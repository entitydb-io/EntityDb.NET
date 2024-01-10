using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources.Queries.Standard;
using System.Collections.Immutable;

namespace EntityDb.Common.Extensions;

/// <summary>
///     Extensions for <see cref="ISourceRepository" />.
/// </summary>
public static class SourceRepositoryExtensions
{
    /// <summary>
    ///     Enumerate source ids for any supported query type
    /// </summary>
    /// <param name="sourceRepository">The source repository</param>
    /// <param name="query">The query</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The source id which are found by <paramref name="query" /></returns>
    public static IAsyncEnumerable<Id> EnumerateSourceIds(this ISourceRepository sourceRepository,
        IQuery query, CancellationToken cancellationToken = default)
    {
        return query switch
        {
            IMessageGroupQuery messageGroupQuery => sourceRepository.EnumerateSourceIds(messageGroupQuery,
                cancellationToken),
            IMessageQuery messageQuery => sourceRepository.EnumerateSourceIds(messageQuery, cancellationToken),
            ILeaseQuery leaseQuery => sourceRepository.EnumerateSourceIds(leaseQuery, cancellationToken),
            ITagQuery tagQuery => sourceRepository.EnumerateSourceIds(tagQuery, cancellationToken),
            _ => AsyncEnumerable.Empty<Id>(),
        };
    }

    /// <summary>
    ///     Reconstruct the <see cref="Source" /> object by source id
    /// </summary>
    /// <param name="sourceRepository">The source repository</param>
    /// <param name="sourceId">The source id</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>An instance of <see cref="Source" />.</returns>
    /// <remarks>
    ///     This does *not* initialize any of the following:
    ///     <ul>
    ///         <li>
    ///             <see cref="Message.AddLeases" />
    ///         </li>
    ///         <li>
    ///             <see cref="Message.AddTags" />
    ///         </li>
    ///         <li>
    ///             <see cref="Message.DeleteLeases" />
    ///         </li>
    ///         <li>
    ///             <see cref="Message.DeleteTags" />
    ///         </li>
    ///     </ul>
    ///     They will be empty.
    /// </remarks>
    public static async Task<Source> GetSource
    (
        this ISourceRepository sourceRepository,
        Id sourceId,
        CancellationToken cancellationToken
    )
    {
        var query = new GetSourceQuery(sourceId);

        var annotatedAgentSignature = await sourceRepository
            .EnumerateAnnotatedAgentSignatures(query, cancellationToken)
            .SingleAsync(cancellationToken);

        var messages = await sourceRepository
            .EnumerateAnnotatedDeltas(query, cancellationToken)
            .Select(annotatedDelta => new Message
            {
                Id = annotatedDelta.MessageId,
                EntityPointer = annotatedDelta.EntityPointer,
                Delta = annotatedDelta.Data,
            })
            .ToArrayAsync(cancellationToken);

        return new Source
        {
            Id = annotatedAgentSignature.SourceId,
            TimeStamp = annotatedAgentSignature.SourceTimeStamp,
            AgentSignature = annotatedAgentSignature.Data,
            Messages = messages.ToImmutableArray(),
        };
    }
}
