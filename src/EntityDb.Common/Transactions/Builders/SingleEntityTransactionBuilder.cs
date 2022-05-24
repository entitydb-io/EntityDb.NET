using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Builders;

/// <summary>
///     A transaction builder for a single entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class SingleEntityTransactionBuilder<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly TransactionBuilder<TEntity> _transactionBuilder;
    
    /// <summary>
    ///     The id used for all transaction builder methods, where applicable.
    /// </summary>
    public Id EntityId { get; }

    internal SingleEntityTransactionBuilder(TransactionBuilder<TEntity> transactionBuilder, Id entityId)
    {
        _transactionBuilder = transactionBuilder;
        EntityId = entityId;
    }

    /// <summary>
    ///     Returns a <typeparamref name="TEntity"/>, if it is known.
    /// </summary>
    /// <returns>A <typeparamref name="TEntity"/>, if it is known.</returns>
    public TEntity GetEntity()
    {
        return _transactionBuilder.GetEntity(EntityId);
    }

    /// <summary>
    ///     Indicates whether or not a <typeparamref name="TEntity"/> is in memory (i.e., created or loaded).
    /// </summary>
    /// <returns><c>true</c> if a <typeparamref name="TEntity"/> is in memory, or else <c>false</c>.</returns>
    public bool IsEntityKnown()
    {
        return _transactionBuilder.IsEntityKnown(EntityId);
    }

    /// <summary>
    ///     Associate a <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="entity">A <typeparamref name="TEntity"/></param>
    /// <returns>The transaction builder.</returns>
    /// <remarks>
    ///     Call this method to load an entity that already exists before calling
    ///     <see cref="Append(object)"/>.
    /// </remarks>
    public SingleEntityTransactionBuilder<TEntity> Load(TEntity entity)
    {
        _transactionBuilder.Load(EntityId, entity);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that appends a single command.
    /// </summary>
    /// <param name="command">The new command that modifies the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public SingleEntityTransactionBuilder<TEntity> Append(object command)
    {
        _transactionBuilder.Append(EntityId, command);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that adds a set of <see cref="ILease"/>s.
    /// </summary>
    /// <param name="leases">The leases to be added to the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public SingleEntityTransactionBuilder<TEntity> Add(params ILease[] leases)
    {
        _transactionBuilder.Add(EntityId, leases);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that adds a set of <see cref="ITag"/>s.
    /// </summary>
    /// <param name="tags">The tags to be added to the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public SingleEntityTransactionBuilder<TEntity> Add(params ITag[] tags)
    {
        _transactionBuilder.Add(EntityId, tags);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that deletes a set of <see cref="ILease"/>s.
    /// </summary>
    /// <param name="leases">The leases to be deleted from the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public SingleEntityTransactionBuilder<TEntity> Delete(params ILease[] leases)
    {
        _transactionBuilder.Delete(EntityId, leases);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that deletes a set of <see cref="ITag"/>s.
    /// </summary>
    /// <param name="tags">The tags to be deleted from the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public SingleEntityTransactionBuilder<TEntity> Delete(params ITag[] tags)
    {
        _transactionBuilder.Delete(EntityId, tags);

        return this;
    }

    /// <summary>
    ///     Returns a new instance of <see cref="ITransaction" />.
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="transactionId">A new id for the new transaction.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ITransaction" />.</returns>
    /// <remarks>
    ///     Note that this is just a proxy for a <see cref="TransactionBuilder{TEntity}"/>,
    ///     and does NOT filter out steps for entity ids not associated with this
    ///     <see cref="SingleEntityTransactionBuilder{TEntity}"/>.
    /// </remarks>
    public Task<ITransaction> BuildAsync(string agentSignatureOptionsName, Id transactionId, CancellationToken cancellationToken = default)
    {
        return _transactionBuilder.BuildAsync(agentSignatureOptionsName, transactionId, cancellationToken);
    }
}
