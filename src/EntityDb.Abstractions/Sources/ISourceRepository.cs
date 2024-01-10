using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Sources;

/// <summary>
///     Represents an explicit set of objects which represent a complete history of a set of entities.
/// </summary>
public interface ISourceRepository : IDisposableResource
{
    /// <summary>
    ///     Returns the source ids which are found by a message group query.
    /// </summary>
    /// <param name="sourceDataDataQuery">The source data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The source ids which are found by <paramref name="sourceDataDataQuery" />.</returns>
    IAsyncEnumerable<Id> EnumerateSourceIds(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the source ids which are found by a message query.
    /// </summary>
    /// <param name="messageDataDataQuery">The message data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The source ids which are found by <paramref name="messageDataDataQuery" />.</returns>
    IAsyncEnumerable<Id> EnumerateSourceIds(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the source ids which are found by a lease query.
    /// </summary>
    /// <param name="leaseDataDataQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The source ids which are found by <paramref name="leaseDataDataQuery" />.</returns>
    IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the source ids which are found by a tag query.
    /// </summary>
    /// <param name="tagDataDataQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The source ids which are found by <paramref name="tagDataDataQuery" />.</returns>
    IAsyncEnumerable<Id> EnumerateSourceIds(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the state pointers which are found by a agentSignature query.
    /// </summary>
    /// <param name="sourceDataDataQuery">The source data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state pointers which are found by <paramref name="sourceDataDataQuery" />.</returns>
    IAsyncEnumerable<Pointer> EnumerateStatePointers(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the state pointers which are found by a message query.
    /// </summary>
    /// <param name="messageDataDataQuery">The message data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state pointers which are found by <paramref name="messageDataDataQuery" />.</returns>
    IAsyncEnumerable<Pointer> EnumerateStatePointers(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the state pointers which are found by a lease query.
    /// </summary>
    /// <param name="leaseDataDataQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state pointers which are found by <paramref name="leaseDataDataQuery" />.</returns>
    IAsyncEnumerable<Pointer> EnumerateStatePointers(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the state pointers which are found by a tag query.
    /// </summary>
    /// <param name="tagDataDataQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state pointers which are found by <paramref name="tagDataDataQuery" />.</returns>
    IAsyncEnumerable<Pointer> EnumerateStatePointers(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the agentSignatures which are found by an message group query.
    /// </summary>
    /// <param name="sourceDataDataQuery">The source data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The agent signatures which are found by <paramref name="sourceDataDataQuery" />.</returns>
    IAsyncEnumerable<object> EnumerateAgentSignatures(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the deltas which are found by a message query.
    /// </summary>
    /// <param name="messageDataDataQuery">The message data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The deltas which are found by <paramref name="messageDataDataQuery" />.</returns>
    IAsyncEnumerable<object> EnumerateDeltas(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the leases which are found by a lease query.
    /// </summary>
    /// <param name="leaseDataDataQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The leases which are found by <paramref name="leaseDataDataQuery" />.</returns>
    IAsyncEnumerable<ILease> EnumerateLeases(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the tags which are found by a tag query.
    /// </summary>
    /// <param name="tagDataDataQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The tags which are found by <paramref name="tagDataDataQuery" />.</returns>
    IAsyncEnumerable<ITag> EnumerateTags(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the annotated agent signatures which are found by a message group query.
    /// </summary>
    /// <param name="sourceDataDataQuery">The source data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The annotated agent signatures which are found by <paramref name="sourceDataDataQuery" />.</returns>
    IAsyncEnumerable<IAnnotatedSourceData<object>> EnumerateAnnotatedAgentSignatures(
        ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the annotated deltas which are found by a message query.
    /// </summary>
    /// <param name="messageDataDataQuery">The message data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The annotated deltas which are found by <paramref name="messageDataDataQuery" />.</returns>
    IAsyncEnumerable<IAnnotatedMessageData<object>> EnumerateAnnotatedDeltas(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Atomically commits a single source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Returns <c>true</c> unless the source cannot be committed.</returns>
    Task<bool> Commit(Source source, CancellationToken cancellationToken = default);
}
