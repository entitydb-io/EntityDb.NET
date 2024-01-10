using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Entities;

internal class EntityRepositoryFactory<TEntity> : IEntityRepositoryFactory<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAgentAccessor _agentAccessor;
    private readonly ISnapshotRepositoryFactory<TEntity>? _snapshotRepositoryFactory;
    private readonly ISourceRepositoryFactory _sourceRepositoryFactory;

    public EntityRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IAgentAccessor agentAccessor,
        ISourceRepositoryFactory sourceRepositoryFactory,
        ISnapshotRepositoryFactory<TEntity>? snapshotRepositoryFactory = null
    )
    {
        _serviceProvider = serviceProvider;
        _agentAccessor = agentAccessor;
        _sourceRepositoryFactory = sourceRepositoryFactory;
        _snapshotRepositoryFactory = snapshotRepositoryFactory;
    }

    public async Task<ISingleEntityRepository<TEntity>> CreateSingleForNew
    (
        Id entityId,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? snapshotSessionOptionsName = null,
        CancellationToken cancellationToken = default
    )
    {
        var multipleEntityRepository = await CreateMultiple(agentSignatureOptionsName, sourceSessionOptionsName,
            snapshotSessionOptionsName, cancellationToken);

        multipleEntityRepository.Create(entityId);

        return new SingleEntityRepository<TEntity>(multipleEntityRepository, entityId);
    }
    
    public async Task<ISingleEntityRepository<TEntity>> CreateSingleForExisting
    (
        Pointer entityPointer,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? snapshotSessionOptionsName = null,
        CancellationToken cancellationToken = default
    )
    {
        var multipleEntityRepository = await CreateMultiple(agentSignatureOptionsName, sourceSessionOptionsName,
            snapshotSessionOptionsName, cancellationToken);

        await multipleEntityRepository.Load(entityPointer, cancellationToken);
        
        return new SingleEntityRepository<TEntity>(multipleEntityRepository, entityPointer);
    }
    
    public async Task<IMultipleEntityRepository<TEntity>> CreateMultiple
    (
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        string? snapshotSessionOptionsName = null,
        CancellationToken cancellationToken = default
    )
    {
        var agent = await _agentAccessor.GetAgent(agentSignatureOptionsName, cancellationToken);
        
        var sourceRepository = await _sourceRepositoryFactory
            .CreateRepository(sourceSessionOptionsName, cancellationToken);

        if (_snapshotRepositoryFactory is null || snapshotSessionOptionsName is null)
        {
            return MultipleEntityRepository<TEntity>.Create
            (
                _serviceProvider,
                agent,
                sourceRepository
            );
        }

        var snapshotRepository = await _snapshotRepositoryFactory
            .CreateRepository(snapshotSessionOptionsName, cancellationToken);

        return MultipleEntityRepository<TEntity>.Create
        (
            _serviceProvider,
            agent,
            sourceRepository,
            snapshotRepository
        );
    }
}
