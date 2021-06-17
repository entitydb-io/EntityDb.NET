using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    /// Represents an explicit set of objects which represent a complete history of a set of entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity represented by the objects stored in the <see cref="ITransactionRepository{TEntity}"/>.</typeparam>
    public interface ITransactionRepository<TEntity> : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Returns the transaction ids which are found by a source query.
        /// </summary>
        /// <param name="sourceQuery">The source query.</param>
        /// <returns>The transaction ids which are found by <paramref name="sourceQuery"/>.</returns>
        Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery);

        /// <summary>
        /// Returns the transaction ids which are found by a command query.
        /// </summary>
        /// <param name="commandQuery">The command query.</param>
        /// <returns>The transaction ids which are found by <paramref name="commandQuery"/>.</returns>
        Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery);

        /// <summary>
        /// Returns the transaction ids which are found by a fact query.
        /// </summary>
        /// <param name="factQuery">The fact query.</param>
        /// <returns>The transaction ids which are found by <paramref name="factQuery"/>.</returns>
        Task<Guid[]> GetTransactionIds(IFactQuery factQuery);

        /// <summary>
        /// Returns the transaction ids which are found by a tag query.
        /// </summary>
        /// <param name="tagQuery">The tag query.</param>
        /// <returns>The transaction ids which are found by <paramref name="tagQuery"/>.</returns>
        Task<Guid[]> GetTransactionIds(ITagQuery tagQuery);

        /// <summary>
        /// Returns the entity ids which are found by a source query.
        /// </summary>
        /// <param name="sourceQuery">The source query.</param>
        /// <returns>The entity ids which are found by <paramref name="sourceQuery"/>.</returns>
        Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery);

        /// <summary>
        /// Returns the entity ids which are found by a command query.
        /// </summary>
        /// <param name="commandQuery">The command query.</param>
        /// <returns>The entity ids which are found by <paramref name="commandQuery"/>.</returns>
        Task<Guid[]> GetEntityIds(ICommandQuery commandQuery);

        /// <summary>
        /// Returns the entity ids which are found by a fact query.
        /// </summary>
        /// <param name="factQuery">The fact query.</param>
        /// <returns>The entity ids which are found by <paramref name="factQuery"/>.</returns>
        Task<Guid[]> GetEntityIds(IFactQuery factQuery);

        /// <summary>
        /// Returns the entity ids which are found by a tag query.
        /// </summary>
        /// <param name="tagQuery">The tag query.</param>
        /// <returns>The entity ids which are found by <paramref name="tagQuery"/>.</returns>
        Task<Guid[]> GetEntityIds(ITagQuery tagQuery);

        /// <summary>
        /// Returns the sources which are found by a source query.
        /// </summary>
        /// <param name="sourceQuery">The source query.</param>
        /// <returns>The sources which are found by <paramref name="sourceQuery"/>.</returns>
        Task<object[]> GetSources(ISourceQuery sourceQuery);

        /// <summary>
        /// Returns the commands which are found by a command query.
        /// </summary>
        /// <param name="commandQuery">The command query.</param>
        /// <returns>The commands which are found by <paramref name="commandQuery"/>.</returns>
        Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery);

        /// <summary>
        /// Returns the facts which are found by a fact query.
        /// </summary>
        /// <param name="factQuery">The fact query.</param>
        /// <returns>The facts which are found by <paramref name="factQuery"/>.</returns>
        Task<IFact<TEntity>[]> GetFacts(IFactQuery factQuery);

        /// <summary>
        /// Returns the tags which are found by a tag query.
        /// </summary>
        /// <param name="tagQuery">The tag query.</param>
        /// <returns>The tags which are found by <paramref name="tagQuery"/>.</returns>
        Task<ITag[]> GetTags(ITagQuery tagQuery);

        /// <summary>
        /// Inserts a single transaction with an atomic commit.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns><c>true</c> if the insert suceeded, or <c>false</c> if the insert failed.</returns>
        Task<bool> PutTransaction(ITransaction<TEntity> transaction);
    }
}
