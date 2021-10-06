using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Entities
{
    /// <summary>
    ///     Represents a type used to create instances of <see cref="IEntityRepository{TEntity}" />
    /// </summary>
    /// <typeparam name="TEntity">The type of entity managed by the <see cref="IEntityRepository{TEntity}" />.</typeparam>
    public interface IEntityRepositoryFactory<TEntity>
    {
        /// <summary>
        ///     Create a new instance of <see cref="IEntityRepository{TEntity}" />
        /// </summary>
        /// <param name="transactionSessionOptions">The agent's use case for the inner transaction repository.</param>
        /// <param name="snapshotSessionOptions">The agent's use case for the inner snapshot repository.</param>
        /// <returns>A new instance of <see cref="IEntityRepository{TEntity}" />.</returns>
        Task<IEntityRepository<TEntity>> CreateRepository(ITransactionSessionOptions transactionSessionOptions,
            ISnapshotSessionOptions? snapshotSessionOptions = null);
    }
}
