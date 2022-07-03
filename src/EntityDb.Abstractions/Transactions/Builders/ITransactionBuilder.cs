using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Transactions.Builders;

/// <summary>
///     Provides a way to construct an <see cref="ITransaction" />. Note that no operations are permanent until
///     you call <see cref="Build(Id)" /> and pass the result to a transaction repository.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the transaction.</typeparam>
public interface ITransactionBuilder<TEntity>
{
    /// <summary>
    ///     Returns a <typeparamref name="TEntity" /> associated with a given entity id, if it is known.
    /// </summary>
    /// <param name="entityId">The id associated with the entity.</param>
    /// <returns>A <typeparamref name="TEntity" /> associated with <paramref name="entityId" />, if it is known.</returns>
    TEntity GetEntity(Id entityId);

    /// <summary>
    ///     Indicates whether or not a <typeparamref name="TEntity" /> associated with a given entity id is in memory.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <returns>
    ///     <c>true</c> if a <typeparamref name="TEntity" /> associated with <paramref name="entityId" /> is in memory, or
    ///     else <c>false</c>.
    /// </returns>
    bool IsEntityKnown(Id entityId);

    /// <summary>
    ///     Associate a <typeparamref name="TEntity" /> with a given entity id.
    /// </summary>
    /// <param name="entityId">An id associated with a <typeparamref name="TEntity" />.</param>
    /// <param name="entity">A <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    /// <remarks>
    ///     Call this method to load an entity that already exists before calling
    ///     <see cref="Append(Id, object)" />.
    /// </remarks>
    ITransactionBuilder<TEntity> Load(Id entityId, TEntity entity);

    /// <summary>
    ///     Adds a transaction step that appends a single command associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity" />.</param>
    /// <param name="command">The new command that modifies the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ITransactionBuilder<TEntity> Append(Id entityId, object command);

    /// <summary>
    ///     Adds a transaction step that adds a set of <see cref="ILease" />s associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity" />.</param>
    /// <param name="leases">The leases to be added to the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ITransactionBuilder<TEntity> Add(Id entityId, params ILease[] leases);

    /// <summary>
    ///     Adds a transaction step that adds a set of <see cref="ITag" />s associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity" />.</param>
    /// <param name="tags">The tags to be added to the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ITransactionBuilder<TEntity> Add(Id entityId, params ITag[] tags);

    /// <summary>
    ///     Adds a transaction step that deletes a set of <see cref="ILease" />s associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity" />.</param>
    /// <param name="leases">The leases to be deleted from the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ITransactionBuilder<TEntity> Delete(Id entityId, params ILease[] leases);

    /// <summary>
    ///     Adds a transaction step that deletes a set of <see cref="ITag" />s associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity" />.</param>
    /// <param name="tags">The tags to be deleted from the <typeparamref name="TEntity" />.</param>
    /// <returns>The transaction builder.</returns>
    ITransactionBuilder<TEntity> Delete(Id entityId, params ITag[] tags);

    /// <summary>
    ///     Returns a new instance of <see cref="ITransaction" />.
    /// </summary>
    /// <param name="transactionId">A new id for the new transaction.</param>
    /// <returns>A new instance of <see cref="ITransaction" />.</returns>
    ITransaction Build(Id transactionId);
}
