using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Transactions.Steps;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions.Builders;

internal sealed class TransactionBuilder<TEntity> : ITransactionBuilder<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly Dictionary<Id, TEntity> _knownEntities = new();
    private readonly List<ITransactionStep> _transactionSteps = new();
    private readonly IAgent _agent;

    public TransactionBuilder(IAgent agent)
    {
        _agent = agent;
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

    public TEntity GetEntity(Id entityId)
    {
        return _knownEntities[entityId];
    }

    public bool IsEntityKnown(Id entityId)
    {
        return _knownEntities.ContainsKey(entityId);
    }

    public ITransactionBuilder<TEntity> Load(Id entityId, TEntity entity)
    {
        if (IsEntityKnown(entityId))
        {
            throw new EntityAlreadyKnownException();
        }

        _knownEntities.Add(entityId, entity);

        return this;
    }

    public ITransactionBuilder<TEntity> Append(Id entityId, object command)
    {
        ConstructIfNotKnown(entityId);

        var previousEntity = _knownEntities[entityId];
        var previousEntityVersionNumber = previousEntity.GetVersionNumber();

        var entity = previousEntity.Reduce(command);
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new AppendCommandTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Command = command,
            PreviousEntityVersionNumber = previousEntityVersionNumber
        });
        
        _knownEntities[entityId] = entity;

        return this;
    }

    public ITransactionBuilder<TEntity> Add(Id entityId, params ILease[] leases)
    {
        ConstructIfNotKnown(entityId);

        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new AddLeasesTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Leases = leases.ToImmutableArray()
        });

        return this;
    }

    public ITransactionBuilder<TEntity> Add(Id entityId, params ITag[] tags)
    {
        ConstructIfNotKnown(entityId);

        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new AddTagsTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Tags = tags.ToImmutableArray()
        });

        return this;
    }

    public ITransactionBuilder<TEntity> Delete(Id entityId, params ILease[] leases)
    {
        ConstructIfNotKnown(entityId);

        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new DeleteLeasesTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Leases = leases.ToImmutableArray()
        });

        return this;
    }

    public ITransactionBuilder<TEntity> Delete(Id entityId, params ITag[] tags)
    {
        ConstructIfNotKnown(entityId);

        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new DeleteTagsTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Tags = tags.ToImmutableArray()
        });

        return this;
    }

    public ITransaction Build(Id transactionId)
    {
        var transaction = new Transaction
        {
            Id = transactionId,
            TimeStamp = _agent.TimeStamp,
            AgentSignature = _agent.Signature,
            Steps = _transactionSteps.ToImmutableArray()
        };

        _transactionSteps.Clear();

        return transaction;
    }
}
