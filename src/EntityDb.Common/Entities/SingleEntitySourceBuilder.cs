using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Entities;

internal sealed class SingleEntitySourceBuilder<TEntity> : ISingleEntitySourceBuilder<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IEntitySourceBuilder<TEntity> _sourceBuilder;

    internal SingleEntitySourceBuilder(IEntitySourceBuilder<TEntity> sourceBuilder, Id entityId)
    {
        _sourceBuilder = sourceBuilder;
        EntityId = entityId;
    }

    public Id EntityId { get; }

    public TEntity GetEntity()
    {
        return _sourceBuilder.GetEntity(EntityId);
    }

    public bool IsEntityKnown()
    {
        return _sourceBuilder.IsEntityKnown(EntityId);
    }

    public ISingleEntitySourceBuilder<TEntity> Load(TEntity entity)
    {
        _sourceBuilder.Load(EntityId, entity);

        return this;
    }

    public ISingleEntitySourceBuilder<TEntity> Append(object delta)
    {
        _sourceBuilder.Append(EntityId, delta);

        return this;
    }

    public Source Build(Id sourceId)
    {
        return _sourceBuilder.Build(sourceId);
    }
}
