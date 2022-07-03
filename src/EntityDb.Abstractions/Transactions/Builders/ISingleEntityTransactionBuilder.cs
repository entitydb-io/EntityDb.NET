using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Transactions.Builders;

/// <summary>
///     A transaction builder for a single entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface ISingleEntityTransactionBuilder<TEntity>
{
    /// <summary>
    ///     The id used for all transaction builder methods, where applicable.
    /// </summary>
    Id EntityId { get; }

    /// <summary>
    ///     Returns a <typeparamref name="TEntity" />, if it is known.
    /// </summary>
    /// <returns>A <typeparamref name="TEntity" />, if it is known.</returns>
    TEntity GetEntity();

    /// <summary>
    ///     Indicates whether or not a <typeparamref name="TEntity" /> is in memory (i.e., created or loaded).
    /// </summary>
    /// <returns><c>true</c> if a <typeparamref name="TEntity" /> is in memory, or else <c>false</c>.</returns>
    bool IsEntityKnown();

    /// <summary>
    ///     Associate a <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="entity">A <typeparamref name="TEntity" /></param>
    /// <returns>The transaction builder.</returns>
    /// <remarks>
    ///     Call this method to load an entity that already exists before calling
    ///     <see cref="Append(object)" />.
    /// </remarks>
    ISingleEntityTransactionBuilder<TEntity> Load(TEntity entity);

    /// <summary>
    ///     Adds a transaction step that appends a single command.
    /// </summary>
    /// <param name="command">The new command that modifies the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ISingleEntityTransactionBuilder<TEntity> Append(object command);

    /// <summary>
    ///     Adds a transaction step that adds a set of <see cref="ILease" />s.
    /// </summary>
    /// <param name="leases">The leases to be added to the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ISingleEntityTransactionBuilder<TEntity> Add(params ILease[] leases);

    /// <summary>
    ///     Adds a transaction step that adds a set of <see cref="ITag" />s.
    /// </summary>
    /// <param name="tags">The tags to be added to the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ISingleEntityTransactionBuilder<TEntity> Add(params ITag[] tags);

    /// <summary>
    ///     Adds a transaction step that deletes a set of <see cref="ILease" />s.
    /// </summary>
    /// <param name="leases">The leases to be deleted from the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ISingleEntityTransactionBuilder<TEntity> Delete(params ILease[] leases);

    /// <summary>
    ///     Adds a transaction step that deletes a set of <see cref="ITag" />s.
    /// </summary>
    /// <param name="tags">The tags to be deleted from the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ISingleEntityTransactionBuilder<TEntity> Delete(params ITag[] tags);

    /// <summary>
    ///     Returns a new instance of <see cref="ITransaction" />.
    /// </summary>
    /// <param name="transactionId">A new id for the new transaction.</param>
    /// <returns>A new instance of <see cref="ITransaction" />.</returns>
    ITransaction Build(Id transactionId);
}
