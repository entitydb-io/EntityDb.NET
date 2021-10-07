using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    ///     Represents an explicit set of objects which represent a complete history of a set of entities.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of entity represented by the objects stored in the
    ///     <see cref="ITransactionRepository{TEntity}" />.
    /// </typeparam>
    public interface ITransactionRepository<TEntity> : IDisposable, IAsyncDisposable
    {
        /// <summary>
        ///     Returns the transaction ids which are found by a source query.
        /// </summary>
        /// <param name="sourceQuery">The source query.</param>
        /// <returns>The transaction ids which are found by <paramref name="sourceQuery" />.</returns>
        Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery);

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
        ///     Returns the entity ids which are found by a source query.
        /// </summary>
        /// <param name="sourceQuery">The source query.</param>
        /// <returns>The entity ids which are found by <paramref name="sourceQuery" />.</returns>
        Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery);

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
        ///     Returns the sources which are found by a source query.
        /// </summary>
        /// <param name="sourceQuery">The source query.</param>
        /// <returns>The sources which are found by <paramref name="sourceQuery" />.</returns>
        Task<object[]> GetSources(ISourceQuery sourceQuery);

        /// <summary>
        ///     Returns the commands which are found by a command query.
        /// </summary>
        /// <param name="commandQuery">The command query.</param>
        /// <returns>The commands which are found by <paramref name="commandQuery" />.</returns>
        Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery);

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
        Task<IAnnotatedCommand<TEntity>[]> GetAnnotatedCommands(ICommandQuery commandQuery);

        /// <summary>
        ///     Inserts a single transaction with an atomic commit.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns><c>true</c> if the insert succeeded, or <c>false</c> if the insert failed.</returns>
        Task<bool> PutTransaction(ITransaction<TEntity> transaction);
    }
}
