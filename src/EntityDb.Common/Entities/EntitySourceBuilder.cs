using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Entities.Deltas;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using System.Collections.Immutable;

namespace EntityDb.Common.Entities;

internal sealed class EntitySourceBuilder<TEntity> : IEntitySourceBuilder<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IAgent _agent;
    private readonly Dictionary<Id, TEntity> _knownEntities = new();
    private readonly List<Message> _sourceMessages = new();

    public EntitySourceBuilder(IAgent agent)
    {
        _agent = agent;
    }

    public TEntity GetEntity(Id entityId)
    {
        return _knownEntities[entityId];
    }

    public bool IsEntityKnown(Id entityId)
    {
        return _knownEntities.ContainsKey(entityId);
    }

    public IEntitySourceBuilder<TEntity> Load(Id entityId, TEntity entity)
    {
        if (IsEntityKnown(entityId))
        {
            throw new EntityBranchAlreadyLoadedException();
        }

        _knownEntities.Add(entityId, entity);

        return this;
    }

    public IEntitySourceBuilder<TEntity> Append(Id entityId, object delta)
    {
        ConstructIfNotKnown(entityId);

        var entity = _knownEntities[entityId].Reduce(delta);

        _sourceMessages.Add(new Message
        {
            EntityPointer = entity.GetPointer(),
            Delta = delta,
            AddLeases = delta is IAddLeasesDelta<TEntity> addLeasesDelta
                ? addLeasesDelta.GetLeases(entity).ToImmutableArray()
                : ImmutableArray<ILease>.Empty,
            AddTags = delta is IAddTagsDelta<TEntity> addTagsDelta
                ? addTagsDelta.GetTags(entity).ToImmutableArray()
                : ImmutableArray<ITag>.Empty,
            DeleteLeases = delta is IDeleteLeasesDelta<TEntity> deleteLeasesDelta
                ? deleteLeasesDelta.GetLeases(entity).ToImmutableArray()
                : ImmutableArray<ILease>.Empty,
            DeleteTags = delta is IDeleteTagsDelta<TEntity> deleteTagsDelta
                ? deleteTagsDelta.GetTags(entity).ToImmutableArray()
                : ImmutableArray<ITag>.Empty,
        });

        _knownEntities[entityId] = entity;

        return this;
    }

    public Source Build(Id sourceId)
    {
        var source = new Source
        {
            Id = sourceId,
            TimeStamp = _agent.TimeStamp,
            AgentSignature = _agent.Signature,
            Messages = _sourceMessages.ToImmutableArray(),
        };

        _sourceMessages.Clear();

        return source;
    }

    private void ConstructIfNotKnown(Id entityId)
    {
        if (IsEntityKnown(entityId))
        {
            return;
        }

        var entity = TEntity.Construct(entityId);

        _knownEntities.Add(entityId, entity);
    }
}
