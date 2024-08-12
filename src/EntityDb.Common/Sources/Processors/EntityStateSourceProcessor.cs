using EntityDb.Abstractions;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.States;
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
        if (source.Messages.Any(message => !TEntity.CanReduce(message.Delta)))
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

        var persistEntities = new Dictionary<StatePointer, TEntity>();

        var entityMessageGroups = source.Messages
            .GroupBy(message => message.StatePointer.Id);

        foreach (var entityMessageGroup in entityMessageGroups)
        {
            var entityId = entityMessageGroup.Key;
            var messages = entityMessageGroup.ToArray();

            StatePointer previousEntityPointer = messages[0].StatePointer.Previous();
            
            TEntity previousEntity;
            
            if (previousEntityPointer.StateVersion == StateVersion.Zero)
            {
                previousEntity = TEntity.Construct(entityId);
            }
            else if (await entityRepository.TryLoad(previousEntityPointer, cancellationToken))
            {
                previousEntity = entityRepository.Get(entityId);
            }
            else
            {
                throw new NotSupportedException();
            }
            
            foreach (var message in messages)
            {
                var nextEntity = previousEntity.Reduce(message.Delta);

                if (nextEntity.ShouldPersist())
                {
                    persistEntities[nextEntity.GetPointer()] = nextEntity;
                }
                
                if (nextEntity.ShouldPersistAsLatest())
                {
                    persistEntities[entityId] = nextEntity;
                }

                previousEntity = nextEntity;
            }
        }

        foreach (var (entityPointer, entity) in persistEntities)
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
