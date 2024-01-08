using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;

namespace EntityDb.Common.Projections;

internal class ProjectionRepositoryFactory<TProjection> : IProjectionRepositoryFactory<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISnapshotRepositoryFactory<TProjection>? _snapshotRepositoryFactory;

    public ProjectionRepositoryFactory
    (
        IServiceProvider serviceProvider,
        ISnapshotRepositoryFactory<TProjection>? snapshotRepositoryFactory = null
    )
    {
        _serviceProvider = serviceProvider;
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
    }

    public async Task<IProjectionRepository<TProjection>> CreateRepository
    (
        string? snapshotSessionOptionsName = null,
        CancellationToken cancellationToken = default
    )
    {
        if (_snapshotRepositoryFactory is null || snapshotSessionOptionsName is null)
        {
            return ProjectionRepository<TProjection>.Create(_serviceProvider);
        }

        var snapshotRepository =
            await _snapshotRepositoryFactory.CreateRepository(snapshotSessionOptionsName, cancellationToken);

        return ProjectionRepository<TProjection>.Create(_serviceProvider,
            snapshotRepository);
    }
}
