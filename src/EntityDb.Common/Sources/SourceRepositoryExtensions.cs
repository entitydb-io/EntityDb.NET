using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources.Queries.Standard;

namespace EntityDb.Common.Sources;

/// <summary>
///     Extensions for <see cref="ISourceRepository" />.
/// </summary>
public static class SourceRepositoryExtensions
{
    /// <summary>
    ///     Enumerate source ids for any supported query type
    /// </summary>
    /// <param name="sourceRepository">The source repository</param>
    /// <param name="dataQuery">The query</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The source id which are found by <paramref name="dataQuery" /></returns>
    public static IAsyncEnumerable<Id> EnumerateSourceIds(this ISourceRepository sourceRepository,
        IDataQuery dataQuery, CancellationToken cancellationToken = default)
    {
        return dataQuery switch
        {
            ISourceDataDataQuery sourceDataQuery => sourceRepository.EnumerateSourceIds(sourceDataQuery, cancellationToken),
            IMessageDataDataQuery messageDataQuery => sourceRepository.EnumerateSourceIds(messageDataQuery,
                cancellationToken),
            ILeaseDataDataQuery leaseDataQuery => sourceRepository.EnumerateSourceIds(leaseDataQuery, cancellationToken),
            ITagDataDataQuery tagDataQuery => sourceRepository.EnumerateSourceIds(tagDataQuery, cancellationToken),
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
        CancellationToken cancellationToken = default
    )
    {
        var query = new GetSourceDataQuery(sourceId);

        var annotatedAgentSignature = await sourceRepository
            .EnumerateAnnotatedAgentSignatures(query, cancellationToken)
            .SingleAsync(cancellationToken);

        var messages = await sourceRepository
            .EnumerateAnnotatedDeltas(query, cancellationToken)
            .Select(annotatedDelta => new Message
            {
                Id = annotatedDelta.MessageId,
                StatePointer = annotatedDelta.StatePointer,
                Delta = annotatedDelta.Data,
            })
            .ToArrayAsync(cancellationToken);

        return new Source
        {
            Id = annotatedAgentSignature.SourceId,
            TimeStamp = annotatedAgentSignature.SourceTimeStamp,
            AgentSignature = annotatedAgentSignature.Data,
            Messages = messages.ToArray(),
        };
    }
}
