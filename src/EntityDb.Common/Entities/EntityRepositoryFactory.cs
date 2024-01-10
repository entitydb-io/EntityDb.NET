using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Entities;

internal class EntityRepositoryFactory<TEntity> : IEntityRepositoryFactory<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IAgentAccessor _agentAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISourceRepositoryFactory _sourceRepositoryFactory;
    private readonly IStateRepositoryFactory<TEntity>? _stateRepositoryFactory;

    public EntityRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IAgentAccessor agentAccessor,
        ISourceRepositoryFactory sourceRepositoryFactory,
        IStateRepositoryFactory<TEntity>? stateRepositoryFactory = null
    )
    {
        _serviceProvider = serviceProvider;
        _agentAccessor = agentAccessor;
        _sourceRepositoryFactory = sourceRepositoryFactory;
        _stateRepositoryFactory = stateRepositoryFactory;
    }

    public async Task<ISingleEntityRepository<TEntity>> CreateSingleForNew
    (
        Id entityId,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? stateSessionOptionsName = null,
        CancellationToken cancellationToken = default
    )
    {
        var multipleEntityRepository = await CreateMultiple(agentSignatureOptionsName, sourceSessionOptionsName,
            stateSessionOptionsName, cancellationToken);

        multipleEntityRepository.Create(entityId);

        return new SingleEntityRepository<TEntity>(multipleEntityRepository, entityId);
    }

    public async Task<ISingleEntityRepository<TEntity>> CreateSingleForExisting
    (
        Pointer entityPointer,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? stateSessionOptionsName = null,
        CancellationToken cancellationToken = default
    )
    {
        var multipleEntityRepository = await CreateMultiple(agentSignatureOptionsName, sourceSessionOptionsName,
            stateSessionOptionsName, cancellationToken);

        await multipleEntityRepository.Load(entityPointer, cancellationToken);

        return new SingleEntityRepository<TEntity>(multipleEntityRepository, entityPointer);
    }

    public async Task<IMultipleEntityRepository<TEntity>> CreateMultiple
    (
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? stateSessionOptionsName = null,
        CancellationToken cancellationToken = default
    )
    {
        var agent = await _agentAccessor.GetAgent(agentSignatureOptionsName, cancellationToken);

        var sourceRepository = await _sourceRepositoryFactory
            .Create(sourceSessionOptionsName, cancellationToken);

        if (_stateRepositoryFactory is null || stateSessionOptionsName is null)
        {
            return MultipleEntityRepository<TEntity>.Create
            (
                _serviceProvider,
                agent,
                sourceRepository
            );
        }

        var stateRepository = await _stateRepositoryFactory
            .Create(stateSessionOptionsName, cancellationToken);

        return MultipleEntityRepository<TEntity>.Create
        (
            _serviceProvider,
            agent,
            sourceRepository,
            stateRepository
        );
    }
}
