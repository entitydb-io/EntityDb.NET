using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Entities;

internal sealed class EntitySourceBuilderFactory<TEntity> : IEntitySourceBuilderFactory<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IAgentAccessor _agentAccessor;

    public EntitySourceBuilderFactory(IAgentAccessor agentAccessor)
    {
        _agentAccessor = agentAccessor;
    }

    public async Task<IEntitySourceBuilder<TEntity>> Create(string agentSignatureOptionsName,
        CancellationToken cancellationToken)
    {
        var agent = await _agentAccessor.GetAgent(agentSignatureOptionsName, cancellationToken);

        return new EntitySourceBuilder<TEntity>(agent);
    }

    public async Task<ISingleEntitySourceBuilder<TEntity>> CreateForSingleEntity(string agentSignatureOptionsName,
        Id entityId, CancellationToken cancellationToken)
    {
        var sourceBuilder = await Create(agentSignatureOptionsName, cancellationToken);

        return new SingleEntitySourceBuilder<TEntity>(sourceBuilder, entityId);
    }
}
