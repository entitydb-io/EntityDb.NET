using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Projections;

internal sealed class ProjectionRepository<TProjection> : DisposableResourceBaseClass,
    IProjectionRepository<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IServiceProvider _serviceProvider;

    public ProjectionRepository
    (
        IServiceProvider serviceProvider,
        ISnapshotRepository<TProjection>? snapshotRepository = null
    )
    {
        _serviceProvider = serviceProvider;
        SnapshotRepository = snapshotRepository;
    }

    public ISnapshotRepository<TProjection>? SnapshotRepository { get; }

    public async Task<TProjection> GetSnapshot(Pointer projectionPointer, CancellationToken cancellationToken = default)
    {
        var projection = SnapshotRepository is not null
            ? await SnapshotRepository.GetSnapshotOrDefault(projectionPointer, cancellationToken) ??
              TProjection.Construct(projectionPointer.Id)
            : TProjection.Construct(projectionPointer.Id);

        var sources = projection.EnumerateSources(_serviceProvider, projectionPointer, cancellationToken);

        await foreach (var source in sources)
        {
            projection.Mutate(source);
        }

        if (!projectionPointer.IsSatisfiedBy(projection.GetPointer()))
        {
            throw new SnapshotPointerDoesNotExistException();
        }

        return projection;
    }

    public static IProjectionRepository<TProjection> Create(IServiceProvider serviceProvider,
        ISnapshotRepository<TProjection>? snapshotRepository = null)
    {
        if (snapshotRepository is null)
        {
            return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider);
        }

        return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
            snapshotRepository);
    }
}
