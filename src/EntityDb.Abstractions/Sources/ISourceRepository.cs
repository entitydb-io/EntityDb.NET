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
    /// <param name="sourceDataQuery">The source data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The source ids which are found by <paramref name="sourceDataQuery" />.</returns>
    IAsyncEnumerable<Id> EnumerateSourceIds(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the source ids which are found by a message query.
    /// </summary>
    /// <param name="messageDataQuery">The message data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The source ids which are found by <paramref name="messageDataQuery" />.</returns>
    IAsyncEnumerable<Id> EnumerateSourceIds(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the source ids which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The source ids which are found by <paramref name="leaseQuery" />.</returns>
    IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the source ids which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The source ids which are found by <paramref name="tagQuery" />.</returns>
    IAsyncEnumerable<Id> EnumerateSourceIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the state pointers which are found by a agentSignature query.
    /// </summary>
    /// <param name="sourceDataQuery">The source data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state pointers which are found by <paramref name="sourceDataQuery" />.</returns>
    IAsyncEnumerable<Pointer> EnumerateStatePointers(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the state pointers which are found by a message query.
    /// </summary>
    /// <param name="messageDataQuery">The message data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state pointers which are found by <paramref name="messageDataQuery" />.</returns>
    IAsyncEnumerable<Pointer> EnumerateStatePointers(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the state pointers which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state pointers which are found by <paramref name="leaseQuery" />.</returns>
    IAsyncEnumerable<Pointer> EnumerateStatePointers(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the state pointers which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The state pointers which are found by <paramref name="tagQuery" />.</returns>
    IAsyncEnumerable<Pointer> EnumerateStatePointers(ITagQuery tagQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the agentSignatures which are found by an message group query.
    /// </summary>
    /// <param name="sourceDataQuery">The source data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The agent signatures which are found by <paramref name="sourceDataQuery" />.</returns>
    IAsyncEnumerable<object> EnumerateAgentSignatures(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the deltas which are found by a message query.
    /// </summary>
    /// <param name="messageDataQuery">The message data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The deltas which are found by <paramref name="messageDataQuery" />.</returns>
    IAsyncEnumerable<object> EnumerateDeltas(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the leases which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The leases which are found by <paramref name="leaseQuery" />.</returns>
    IAsyncEnumerable<ILease> EnumerateLeases(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the tags which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The tags which are found by <paramref name="tagQuery" />.</returns>
    IAsyncEnumerable<ITag> EnumerateTags(ITagQuery tagQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the annotated agent signatures which are found by a message group query.
    /// </summary>
    /// <param name="sourceDataQuery">The source data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The annotated agent signatures which are found by <paramref name="sourceDataQuery" />.</returns>
    IAsyncEnumerable<IAnnotatedSourceData<object>> EnumerateAnnotatedAgentSignatures(
        ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the annotated deltas which are found by a message query.
    /// </summary>
    /// <param name="messageDataQuery">The message data query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The annotated deltas which are found by <paramref name="messageDataQuery" />.</returns>
    IAsyncEnumerable<IAnnotatedMessageData<object>> EnumerateAnnotatedDeltas(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Atomically commits a single source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
    Task<bool> Commit(Source source, CancellationToken cancellationToken = default);
}
