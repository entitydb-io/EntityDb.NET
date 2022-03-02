using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Transactions;

/// <summary>
///     Represents an explicit set of objects which represent a complete history of a set of entities.
/// </summary>
/// <typeparam name="TEntity">
///     The type of entity represented by the objects stored in the
///     <see cref="ITransactionRepository{TEntity}" />.
/// </typeparam>
public interface ITransactionRepository<TEntity> : IDisposableResource
{
    /// <summary>
    ///     Returns the transaction ids which are found by a agentSignature query.
    /// </summary>
    /// <param name="agentSignatureQuery">The agentSignature query.</param>
    /// <returns>The transaction ids which are found by <paramref name="agentSignatureQuery" />.</returns>
    Task<Guid[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery);

    /// <summary>
    ///     Returns the transaction ids which are found by a command query.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <returns>The transaction ids which are found by <paramref name="commandQuery" />.</returns>
    Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery);

    /// <summary>
    ///     Returns the transaction ids which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <returns>The transaction ids which are found by <paramref name="leaseQuery" />.</returns>
    Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery);

    /// <summary>
    ///     Returns the transaction ids which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <returns>The transaction ids which are found by <paramref name="tagQuery" />.</returns>
    Task<Guid[]> GetTransactionIds(ITagQuery tagQuery);

    /// <summary>
    ///     Returns the entity ids which are found by a agentSignature query.
    /// </summary>
    /// <param name="agentSignatureQuery">The agentSignature query.</param>
    /// <returns>The entity ids which are found by <paramref name="agentSignatureQuery" />.</returns>
    Task<Guid[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery);

    /// <summary>
    ///     Returns the entity ids which are found by a command query.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <returns>The entity ids which are found by <paramref name="commandQuery" />.</returns>
    Task<Guid[]> GetEntityIds(ICommandQuery commandQuery);

    /// <summary>
    ///     Returns the entity ids which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <returns>The entity ids which are found by <paramref name="leaseQuery" />.</returns>
    Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery);

    /// <summary>
    ///     Returns the entity ids which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <returns>The entity ids which are found by <paramref name="tagQuery" />.</returns>
    Task<Guid[]> GetEntityIds(ITagQuery tagQuery);

    /// <summary>
    ///     Returns the agentSignatures which are found by a agentSignature query.
    /// </summary>
    /// <param name="agentSignatureQuery">The agentSignature query.</param>
    /// <returns>The agentSignatures which are found by <paramref name="agentSignatureQuery" />.</returns>
    Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery);

    /// <summary>
    ///     Returns the commands which are found by a command query.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <returns>The commands which are found by <paramref name="commandQuery" />.</returns>
    Task<object[]> GetCommands(ICommandQuery commandQuery);

    /// <summary>
    ///     Returns the leases which are found by a lease query.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <returns>The leases which are found by <paramref name="leaseQuery" />.</returns>
    Task<ILease[]> GetLeases(ILeaseQuery leaseQuery);

    /// <summary>
    ///     Returns the tags which are found by a tag query.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <returns>The tags which are found by <paramref name="tagQuery" />.</returns>
    Task<ITag[]> GetTags(ITagQuery tagQuery);

    /// <summary>
    ///     Returns the annotated commands which are found by a command query.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <returns>The annotated commands which are found by <paramref name="commandQuery" />.</returns>
    Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery);

    /// <summary>
    ///     Inserts a single transaction with an atomic commit.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
    Task<bool> PutTransaction(ITransaction<TEntity> transaction);
}
