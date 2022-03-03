using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Transactions;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections;

public class ProjectionRepositoryFactory<TProjection> : IProjectionRepositoryFactory<TProjection>
    where TProjection : IProjection<TProjection>
{
    public ProjectionRepositoryFactory(ITransactionRepositoryFactory transactionRepositoryFactory)
    {
        
    }
    
    public Task<IProjectionRepository<TProjection>> CreateRepository(string transactionSessionOptionsName, string snapshotSessionOptionsName)
    {
        var transactionRepository =
            await _transactionRepositoryFactory.CreateRepository(transactionSessionOptionsName);

        var snapshotRepository = await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName);

        return ProjectionRepository<TProjection, TEntity>.Create(_serviceProvider,
            transactionRepository, snapshotRepository);
    }
}
