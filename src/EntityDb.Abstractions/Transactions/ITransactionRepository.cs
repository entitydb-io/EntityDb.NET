using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Transactions;

/// <summary>
///     Represents an explicit set of objects which represent a complete history of a set of entities.
/// </summary>
public interface ITransactionRepository : IDisposableResource
{
    /// <summary>
    ///     Returns the transaction ids which are found by a agentSignature query.
    /// </summary>
    /// <param name="agentSignatureQuery">The agentSignature query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transaction ids which are found by <paramref name="agentSignatureQuery" />.</returns>
    Task<Id[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the transaction ids which are found by a command query.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transaction ids which are found by <paramref name="commandQuery" />.</returns>
    Task<Id[]> GetTransactionIds(ICommandQuery commandQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the transaction ids which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transaction ids which are found by <paramref name="leaseQuery" />.</returns>
    Task<Id[]> GetTransactionIds(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the transaction ids which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transaction ids which are found by <paramref name="tagQuery" />.</returns>
    Task<Id[]> GetTransactionIds(ITagQuery tagQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the entity ids which are found by a agentSignature query.
    /// </summary>
    /// <param name="agentSignatureQuery">The agentSignature query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entity ids which are found by <paramref name="agentSignatureQuery" />.</returns>
    Task<Id[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the entity ids which are found by a command query.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entity ids which are found by <paramref name="commandQuery" />.</returns>
    Task<Id[]> GetEntityIds(ICommandQuery commandQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the entity ids which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entity ids which are found by <paramref name="leaseQuery" />.</returns>
    Task<Id[]> GetEntityIds(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the entity ids which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entity ids which are found by <paramref name="tagQuery" />.</returns>
    Task<Id[]> GetEntityIds(ITagQuery tagQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the agentSignatures which are found by a agentSignature query.
    /// </summary>
    /// <param name="agentSignatureQuery">The agentSignature query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The agentSignatures which are found by <paramref name="agentSignatureQuery" />.</returns>
    Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the commands which are found by a command query.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The commands which are found by <paramref name="commandQuery" />.</returns>
    Task<object[]> GetCommands(ICommandQuery commandQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the leases which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The leases which are found by <paramref name="leaseQuery" />.</returns>
    Task<ILease[]> GetLeases(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the tags which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The tags which are found by <paramref name="tagQuery" />.</returns>
    Task<ITag[]> GetTags(ITagQuery tagQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the annotated agent signatures which are found by an agent signature query.
    /// </summary>
    /// <param name="agentSignatureQuery">The agent signature query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The annotated agent signatures which are found by <paramref name="agentSignatureQuery" />.</returns>
    Task<IEntitiesAnnotation<object>[]> GetAnnotatedAgentSignatures(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the annotated commands which are found by a command query.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The annotated commands which are found by <paramref name="commandQuery" />.</returns>
    Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Inserts a single transaction with an atomic commit.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
    Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default);
}
