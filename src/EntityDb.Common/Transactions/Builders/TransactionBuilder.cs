using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions.Builders;

internal sealed class TransactionBuilder<TEntity> : ITransactionBuilder<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IAgent _agent;
    private readonly Dictionary<Id, TEntity> _knownEntities = new();
    private readonly List<ITransactionCommand> _transactionCommands = new();

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

        var entity = _knownEntities[entityId].Reduce(command);
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionCommands.Add(new TransactionCommand
        {
            EntityId = entityId,
            EntityVersionNumber = entityVersionNumber,
            Data = command,
        });

        _knownEntities[entityId] = entity;

        return this;
    }

    public ITransaction Build(Id transactionId)
    {
        var transaction = new Transaction
        {
            Id = transactionId,
            TimeStamp = _agent.TimeStamp,
            AgentSignature = _agent.Signature,
            Commands = _transactionCommands.ToImmutableArray()
        };

        _transactionCommands.Clear();

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
}
