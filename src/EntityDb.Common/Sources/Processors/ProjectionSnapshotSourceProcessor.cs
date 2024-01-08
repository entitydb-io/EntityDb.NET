using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Sources.Processors;

/// <summary>
///     A source processor that creates projection snapshots
/// </summary>
/// <typeparam name="TProjection"></typeparam>
public sealed class ProjectionSnapshotSourceProcessor<TProjection> : ISourceProcessor
    where TProjection : IProjection<TProjection>
{
    private readonly ILogger<ProjectionSnapshotSourceProcessor<TProjection>> _logger;
    private readonly IProjectionRepositoryFactory<TProjection> _projectionRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;

    /// <ignore />
    public ProjectionSnapshotSourceProcessor
    (
        ILogger<ProjectionSnapshotSourceProcessor<TProjection>> logger,
        IProjectionRepositoryFactory<TProjection> projectionRepositoryFactory,
        string snapshotSessionOptionsName
    )
    {
        _logger = logger;
        _projectionRepositoryFactory = projectionRepositoryFactory;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    /// <inheritdoc />
    public async Task Process(Source source, CancellationToken cancellationToken)
    {
        await using var projectionRepository = await _projectionRepositoryFactory
            .CreateRepository(_snapshotSessionOptionsName, cancellationToken);

        if (projectionRepository.SnapshotRepository is null)
        {
            _logger.LogWarning("Snapshots not enabled, no point in processing source.");

            return;
        }

        foreach (var entityId in TProjection.EnumerateEntityIds(source))
        {
            var previousProjection = await projectionRepository.SnapshotRepository
                .GetSnapshotOrDefault(entityId, cancellationToken);

            var nextProjection = previousProjection == null
                ? TProjection.Construct(entityId)
                : previousProjection;

            nextProjection.Mutate(source);

            var nextProjectionPointer = nextProjection.GetPointer();

            if (nextProjection.ShouldRecordAsLatest(previousProjection))
            {
                await projectionRepository.SnapshotRepository.PutSnapshot(entityId, nextProjection,
                    cancellationToken);
            }

            if (nextProjection.ShouldRecord())
            {
                await projectionRepository.SnapshotRepository.PutSnapshot(nextProjectionPointer, nextProjection,
                    cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    internal static ProjectionSnapshotSourceProcessor<TProjection> Create(IServiceProvider serviceProvider,
        string snapshotSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<ProjectionSnapshotSourceProcessor<TProjection>>(serviceProvider,
            snapshotSessionOptionsName);
    }
}
