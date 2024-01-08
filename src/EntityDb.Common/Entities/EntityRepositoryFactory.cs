using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;

namespace EntityDb.Common.Entities;

internal class EntityRepositoryFactory<TEntity> : IEntityRepositoryFactory<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISnapshotRepositoryFactory<TEntity>? _snapshotRepositoryFactory;
    private readonly ISourceRepositoryFactory _sourceRepositoryFactory;

    public EntityRepositoryFactory
    (
        IServiceProvider serviceProvider,
        ISourceRepositoryFactory sourceRepositoryFactory,
        ISnapshotRepositoryFactory<TEntity>? snapshotRepositoryFactory = null
    )
    {
        _serviceProvider = serviceProvider;
        _sourceRepositoryFactory = sourceRepositoryFactory;
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
    }

    public async Task<IEntityRepository<TEntity>> CreateRepository(string sourceSessionOptionsName,
        string? snapshotSessionOptionsName = null, CancellationToken cancellationToken = default)
    {
        var sourceRepository =
            await _sourceRepositoryFactory.CreateRepository(sourceSessionOptionsName, cancellationToken);

        if (_snapshotRepositoryFactory is null || snapshotSessionOptionsName is null)
        {
            return EntityRepository<TEntity>.Create(_serviceProvider,
                sourceRepository);
        }

        var snapshotRepository =
            await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName, cancellationToken);

        return EntityRepository<TEntity>.Create(_serviceProvider,
            sourceRepository, snapshotRepository);
    }
}
