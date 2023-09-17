using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Extensions;
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
    private readonly ILogger<EntitySnapshotSourceProcessor<TEntity>> _logger;
    private readonly IEntityRepositoryFactory<TEntity> _entityRepositoryFactory;
    private readonly string _snapshotSessionOptionsName;
    private readonly string _transactionSessionOptionsName;

    /// <ignore />
    public EntitySnapshotSourceProcessor
    (
        ILogger<EntitySnapshotSourceProcessor<TEntity>> logger,
        IEntityRepositoryFactory<TEntity> entityRepositoryFactory,
        string transactionSessionOptionsName,
        string snapshotSessionOptionsName
    )
    {
        _logger = logger;
        _entityRepositoryFactory = entityRepositoryFactory;
        _transactionSessionOptionsName = transactionSessionOptionsName;
        _snapshotSessionOptionsName = snapshotSessionOptionsName;
    }

    /// <inheritdoc />
    public async Task Process(ISource source, CancellationToken cancellationToken)
    {
        if (source is not ITransaction transaction)
        {
            throw new NotSupportedException();
        }

        await using var entityRepository = await _entityRepositoryFactory
            .CreateRepository(_transactionSessionOptionsName, _snapshotSessionOptionsName, cancellationToken);

        if (entityRepository.SnapshotRepository is null)
        {
            _logger.LogWarning("Snapshots not enabled, no point in processing source.");

            return;
        }

        var latestEntities = new Dictionary<Id, TEntity>();
        var saveEntities = new Dictionary<Pointer, TEntity>();

        foreach (var command in transaction.Commands)
        {   
            var entityId = command.EntityId;

            if (!latestEntities.Remove(entityId, out var previousEntity))
            {
                previousEntity = await entityRepository.SnapshotRepository.GetSnapshotOrDefault(entityId, cancellationToken);
            }

            var nextEntity = (previousEntity ?? TEntity.Construct(entityId)).Reduce(command.Data);
            var nextEntityPointer = nextEntity.GetPointer();

            if (nextEntity.ShouldRecordAsLatest(previousEntity))
            {
                saveEntities[entityId] = nextEntity;
            }

            if (nextEntity.ShouldRecord())
            {
                saveEntities[nextEntityPointer] = nextEntity;
            }

            latestEntities[entityId] = nextEntity;

            cancellationToken.ThrowIfCancellationRequested();
        }

        foreach (var (entityPointer, entity) in saveEntities)
        {
            await entityRepository.SnapshotRepository.PutSnapshot(entityPointer, entity, cancellationToken);
        }
    }

    internal static EntitySnapshotSourceProcessor<TEntity> Create(IServiceProvider serviceProvider,
        string transactionSessionOptionsName, string snapshotSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<EntitySnapshotSourceProcessor<TEntity>>(serviceProvider,
            transactionSessionOptionsName, snapshotSessionOptionsName);
    }
}
