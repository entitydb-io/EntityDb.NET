using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections;

internal class ProjectionRepositoryFactory<TProjection> : IProjectionRepositoryFactory<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITransactionRepositoryFactory _transactionRepositoryFactory;
    private readonly ISnapshotRepositoryFactory<TProjection> _snapshotRepositoryFactory;

    public ProjectionRepositoryFactory
    (
        IServiceProvider serviceProvider,
        ITransactionRepositoryFactory transactionRepositoryFactory,
        ISnapshotRepositoryFactory<TProjection> snapshotRepositoryFactory
    )
    {
        _serviceProvider = serviceProvider;
        _transactionRepositoryFactory = transactionRepositoryFactory;
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
    }
    
    public async Task<IProjectionRepository<TProjection>> CreateRepository(string transactionSessionOptionsName, string snapshotSessionOptionsName)
    {
        var transactionRepository =
            await _transactionRepositoryFactory.CreateRepository(transactionSessionOptionsName);

        var snapshotRepository =
            await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName);

        return ProjectionRepository<TProjection>.Create(_serviceProvider,
            transactionRepository, snapshotRepository);
    }
}
