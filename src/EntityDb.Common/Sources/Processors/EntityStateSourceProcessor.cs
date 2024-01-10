using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Sources.Processors;

/// <summary>
///     A source processor that persists entity states
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
public sealed class EntityStateSourceProcessor<TEntity> : ISourceProcessor
    where TEntity : IEntity<TEntity>
{
    private readonly IEntityRepositoryFactory<TEntity> _entityRepositoryFactory;
    private readonly ILogger<EntityStateSourceProcessor<TEntity>> _logger;
    private readonly string _sourceSessionOptionsName;
    private readonly string _stateSessionOptionsName;

    /// <ignore />
    public EntityStateSourceProcessor
    (
        ILogger<EntityStateSourceProcessor<TEntity>> logger,
        IEntityRepositoryFactory<TEntity> entityRepositoryFactory,
        string sourceSessionOptionsName,
        string stateSessionOptionsName
    )
    {
        _logger = logger;
        _entityRepositoryFactory = entityRepositoryFactory;
        _sourceSessionOptionsName = sourceSessionOptionsName;
        _stateSessionOptionsName = stateSessionOptionsName;
    }

    /// <inheritdoc />
    public async Task Process(Source source, CancellationToken cancellationToken)
    {
        if (!source.Messages.Any(message => TEntity.CanReduce(message.Delta)))
        {
            throw new NotSupportedException();
        }

        await using var entityRepository = await _entityRepositoryFactory
            .CreateMultiple(default!, _sourceSessionOptionsName, _stateSessionOptionsName, cancellationToken);

        if (entityRepository.StateRepository is null)
        {
            _logger.LogWarning("State repository not available, skipping source processing.");

            return;
        }

        var latestEntities = new Dictionary<Id, TEntity>();
        var saveEntities = new Dictionary<Pointer, TEntity>();

        foreach (var message in source.Messages)
        {
            var entityPointer = message.StatePointer;

            if (!latestEntities.Remove(entityPointer.Id, out var previousEntity))
            {
                previousEntity =
                    await entityRepository.StateRepository.Get(entityPointer, cancellationToken);
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
            await entityRepository.StateRepository.Put(entityPointer, entity, cancellationToken);
        }
    }

    internal static EntityStateSourceProcessor<TEntity> Create(IServiceProvider serviceProvider,
        string sourceSessionOptionsName, string stateSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<EntityStateSourceProcessor<TEntity>>(serviceProvider,
            sourceSessionOptionsName, stateSessionOptionsName);
    }
}
