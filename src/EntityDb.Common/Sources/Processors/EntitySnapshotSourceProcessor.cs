using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Sources.Processors;

/// <summary>
///     A source processor that creates entity snapshots
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
public sealed class EntitySnapshotSourceProcessor<TEntity> : ISourceProcessor
    where TEntity : IEntity<TEntity>
{
    private readonly IEntityRepositoryFactory<TEntity> _entityRepositoryFactory;
    private readonly ILogger<EntitySnapshotSourceProcessor<TEntity>> _logger;
    private readonly string _snapshotSessionOptionsName;
    private readonly string _sourceSessionOptionsName;

    /// <ignore />
    public EntitySnapshotSourceProcessor
    (
        ILogger<EntitySnapshotSourceProcessor<TEntity>> logger,
        IEntityRepositoryFactory<TEntity> entityRepositoryFactory,
        string sourceSessionOptionsName,
        string snapshotSessionOptionsName
    )
    {
        _logger = logger;
        _entityRepositoryFactory = entityRepositoryFactory;
        _sourceSessionOptionsName = sourceSessionOptionsName;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    /// <inheritdoc />
    public async Task Process(Source source, CancellationToken cancellationToken)
    {
        if (!source.Messages.Any(message => TEntity.CanReduce(message.Delta)))
        {
            throw new NotSupportedException();
        }

        await using var entityRepository = await _entityRepositoryFactory
            .CreateMultiple(default!, _sourceSessionOptionsName, _snapshotSessionOptionsName, cancellationToken);

        if (entityRepository.SnapshotRepository is null)
        {
            _logger.LogWarning("Snapshots not enabled, no point in processing source.");

            return;
        }

        var latestEntities = new Dictionary<Id, TEntity>();
        var saveEntities = new Dictionary<Pointer, TEntity>();

        foreach (var message in source.Messages)
        {
            var entityPointer = message.EntityPointer;

            if (!latestEntities.Remove(entityPointer.Id, out var previousEntity))
            {
                previousEntity =
                    await entityRepository.SnapshotRepository.GetSnapshotOrDefault(entityPointer, cancellationToken);
            }

            var nextEntity = (previousEntity ?? TEntity.Construct(entityPointer.Id)).Reduce(message.Delta);
            var nextEntityPointer = nextEntity.GetPointer();

            if (nextEntity.ShouldRecordAsLatest(previousEntity))
            {
                saveEntities[entityPointer.Id] = nextEntity;
            }

            if (nextEntity.ShouldRecord())
            {
                saveEntities[nextEntityPointer] = nextEntity;
            }

            latestEntities[entityPointer.Id] = nextEntity;

            cancellationToken.ThrowIfCancellationRequested();
        }

        foreach (var (entityPointer, entity) in saveEntities)
        {
            await entityRepository.SnapshotRepository.PutSnapshot(entityPointer, entity, cancellationToken);
        }
    }

    internal static EntitySnapshotSourceProcessor<TEntity> Create(IServiceProvider serviceProvider,
        string sourceSessionOptionsName, string snapshotSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<EntitySnapshotSourceProcessor<TEntity>>(serviceProvider,
            sourceSessionOptionsName, snapshotSessionOptionsName);
    }
}
