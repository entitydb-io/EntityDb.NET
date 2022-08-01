using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Commands;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Transactions.Steps;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions.Builders;

internal sealed class TransactionBuilder<TEntity> : ITransactionBuilder<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IAgent _agent;
    private readonly Dictionary<Id, TEntity> _knownEntities = new();
    private readonly List<ITransactionStep> _transactionSteps = new();

    public TransactionBuilder(IAgent agent)
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

        var nextEntity = previousEntity.Reduce(command);
        var nextEntityVersionNumber = nextEntity.GetVersionNumber();

        _transactionSteps.Add(new AppendCommandTransactionStep
        {
            EntityId = entityId,
            Entity = nextEntity,
            EntityVersionNumber = nextEntityVersionNumber,
            Command = command,
            PreviousEntityVersionNumber = previousEntityVersionNumber
        });

        _knownEntities[entityId] = nextEntity;

        if (command is IAddLeasesCommand<TEntity> addLeasesCommand)
        {
            Add(entityId, addLeasesCommand.GetLeases(previousEntity, nextEntity).ToImmutableArray());
        }

        if (command is IAddTagsCommand<TEntity> addTagsCommand)
        {
            Add(entityId, addTagsCommand.GetTags(previousEntity, nextEntity).ToImmutableArray());
        }

        if (command is IDeleteLeasesCommand<TEntity> deleteLeasesCommand)
        {
            Delete(entityId, deleteLeasesCommand.GetLeases(previousEntity, nextEntity).ToImmutableArray());
        }

        if (command is IDeleteTagsCommand<TEntity> deleteTagsCommand)
        {
            Delete(entityId, deleteTagsCommand.GetTags(previousEntity, nextEntity).ToImmutableArray());
        }

        return this;
    }

    public ITransactionBuilder<TEntity> Add(Id entityId, params ILease[] leases)
    {
        ConstructIfNotKnown(entityId);

        Add(entityId, leases.ToImmutableArray());

        return this;
    }

    public ITransactionBuilder<TEntity> Add(Id entityId, params ITag[] tags)
    {
        ConstructIfNotKnown(entityId);

        Add(entityId, tags.ToImmutableArray());

        return this;
    }

    public ITransactionBuilder<TEntity> Delete(Id entityId, params ILease[] leases)
    {
        ConstructIfNotKnown(entityId);

        Delete(entityId, leases.ToImmutableArray());

        return this;
    }

    public ITransactionBuilder<TEntity> Delete(Id entityId, params ITag[] tags)
    {
        ConstructIfNotKnown(entityId);

        Delete(entityId, tags.ToImmutableArray());

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

    private void ConstructIfNotKnown(Id entityId)
    {
        if (IsEntityKnown(entityId))
        {
            return;
        }

        var entity = TEntity.Construct(entityId);

        _knownEntities.Add(entityId, entity);
    }

    private void Add(Id entityId, ImmutableArray<ILease> leases)
    {
        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new AddLeasesTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Leases = leases
        });
    }

    private void Add(Id entityId, ImmutableArray<ITag> tags)
    {
        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new AddTagsTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Tags = tags
        });
    }

    private void Delete(Id entityId, ImmutableArray<ILease> leases)
    {
        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new DeleteLeasesTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Leases = leases
        });
    }

    private void Delete(Id entityId, ImmutableArray<ITag> tags)
    {
        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new DeleteTagsTransactionStep
        {
            EntityId = entityId,
            Entity = entity,
            EntityVersionNumber = entityVersionNumber,
            Tags = tags
        });
    }
}
