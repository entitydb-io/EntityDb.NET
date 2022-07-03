using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;

namespace EntityDb.Common.Transactions.Builders;

internal sealed class SingleEntityTransactionBuilder<TEntity> : ISingleEntityTransactionBuilder<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly ITransactionBuilder<TEntity> _transactionBuilder;

    public Id EntityId { get; }

    internal SingleEntityTransactionBuilder(ITransactionBuilder<TEntity> transactionBuilder, Id entityId)
    {
        _transactionBuilder = transactionBuilder;
        EntityId = entityId;
    }

    public TEntity GetEntity()
    {
        return _transactionBuilder.GetEntity(EntityId);
    }

    public bool IsEntityKnown()
    {
        return _transactionBuilder.IsEntityKnown(EntityId);
    }

    public ISingleEntityTransactionBuilder<TEntity> Load(TEntity entity)
    {
        _transactionBuilder.Load(EntityId, entity);

        return this;
    }

    public ISingleEntityTransactionBuilder<TEntity> Append(object command)
    {
        _transactionBuilder.Append(EntityId, command);

        return this;
    }

    public ISingleEntityTransactionBuilder<TEntity> Add(params ILease[] leases)
    {
        _transactionBuilder.Add(EntityId, leases);

        return this;
    }

    public ISingleEntityTransactionBuilder<TEntity> Add(params ITag[] tags)
    {
        _transactionBuilder.Add(EntityId, tags);

        return this;
    }

    public ISingleEntityTransactionBuilder<TEntity> Delete(params ILease[] leases)
    {
        _transactionBuilder.Delete(EntityId, leases);

        return this;
    }

    public ISingleEntityTransactionBuilder<TEntity> Delete(params ITag[] tags)
    {
        _transactionBuilder.Delete(EntityId, tags);

        return this;
    }

    public ITransaction Build(Id transactionId)
    {
        return _transactionBuilder.Build(transactionId);
    }
}
