using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Entities
{
    public interface IEntityRepositoryFactory<TEntity>
    {
        Task<IEntityRepository<TEntity>> CreateRepository(ITransactionSessionOptions transactionSessionOptions,
            ISnapshotSessionOptions? snapshotSessionOptions = null);
    }
}
