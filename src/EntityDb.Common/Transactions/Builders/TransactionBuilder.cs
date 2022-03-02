using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Transactions.Steps;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions.Builders;

/// <summary>
///     Provides a way to construct an <see cref="ITransaction{TEntity}" />. Note that no operations are permanent until
///     you call <see cref="Build(string, Guid)" /> and pass the result to a transaction repository.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the transaction.</typeparam>
public sealed class TransactionBuilder<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly Dictionary<Guid, TEntity> _knownEntities = new();
    private readonly List<ITransactionStep<TEntity>> _transactionSteps = new();

    private readonly IAgentAccessor _agentAccessor;

    /// <summary>
    ///     Initializes a new instance of <see cref="TransactionBuilder{TEntity}" />.
    /// </summary>
    public TransactionBuilder
    (
        IAgentAccessor agentAccessor
    )
    {
        _agentAccessor = agentAccessor;
    }

    private void ConstructIfNotKnown(Guid entityId)
    {
        if (IsEntityKnown(entityId))
        {
            return;
        }

        var entity = TEntity.Construct(entityId);

        _knownEntities.Add(entityId, entity);
    }

    private void AddGeneralTransactionStep(Guid entityId, object command)
    {
        ConstructIfNotKnown(entityId);

        var previousEntity = _knownEntities[entityId];
        var previousEntityVersionNumber = previousEntity.GetVersionNumber();

        var nextEntity = previousEntity.Reduce(command);
        var nextEntityVersionNumber = nextEntity.GetVersionNumber();

        _transactionSteps.Add(new CommandTransactionStep<TEntity>
        {
            EntityId = entityId,
            Command = command,
            PreviousEntitySnapshot = previousEntity,
            PreviousEntityVersionNumber = previousEntityVersionNumber,
            NextEntitySnapshot = nextEntity,
            NextEntityVersionNumber = nextEntityVersionNumber
        });

        _knownEntities[entityId] = nextEntity;
    }

    private void AddLeaseTransactionStep(Guid entityId, IEnumerable<ILease> deleteLeases, IEnumerable<ILease> insertLeases)
    {
        ConstructIfNotKnown(entityId);

        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new LeaseTransactionStep<TEntity>
        {
            EntityId = entityId,
            Leases = new TransactionMetaData<ILease>
            {
                Delete = deleteLeases.ToImmutableArray(),
                Insert = insertLeases.ToImmutableArray()
            },
            LeasedAtEntityVersionNumber = entityVersionNumber
        });
    }

    private void AddTagTransactionStep(Guid entityId, IEnumerable<ITag> deleteTags, IEnumerable<ITag> insertTags)
    {
        ConstructIfNotKnown(entityId);

        var entity = _knownEntities[entityId];
        var entityVersionNumber = entity.GetVersionNumber();

        _transactionSteps.Add(new TagTransactionStep<TEntity>
        {
            EntityId = entityId,
            Tags = new TransactionMetaData<ITag>
            {
                Delete = deleteTags.ToImmutableArray(),
                Insert = insertTags.ToImmutableArray()
            },
            TaggedAtEntityVersionNumber = entityVersionNumber
        });
    }

    /// <summary>
    ///     Returns a single-entity transaction builder, which has a simplified set of methods.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <returns>A single-entity transaction builder, which has a simplified set of methods.</returns>
    public SingleEntityTransactionBuilder<TEntity> ForSingleEntity(Guid entityId)
    {
        return new SingleEntityTransactionBuilder<TEntity>(this, entityId);
    }

    /// <summary>
    ///     Returns a <typeparamref name="TEntity"/> associated with a given entity id, if it is known.
    /// </summary>
    /// <param name="entityId">The id associated with the entity.</param>
    /// <returns>A <typeparamref name="TEntity"/> associated with <paramref name="entityId"/>, if it is known.</returns>
    public TEntity GetEntity(Guid entityId)
    {
        return _knownEntities[entityId];
    }

    /// <summary>
    ///     Indicates whether or not a <typeparamref name="TEntity"/> associated with a given entity id is in memory.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <returns><c>true</c> if a <typeparamref name="TEntity"/> associated with <paramref name="entityId"/> is in memory, or else <c>false</c>.</returns>
    public bool IsEntityKnown(Guid entityId)
    {
        return _knownEntities.ContainsKey(entityId);
    }

    /// <summary>
    ///     Associate a <typeparamref name="TEntity"/> with a given entity id.
    /// </summary>
    /// <param name="entityId">An id associated with a <typeparamref name="TEntity"/>.</param>
    /// <param name="entity">A <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    /// <remarks>
    ///     Call this method to load an entity that already exists before calling
    ///     <see cref="Append(Guid, object)" />.
    /// </remarks>
    public TransactionBuilder<TEntity> Load(Guid entityId, TEntity entity)
    {
        if (IsEntityKnown(entityId))
        {
            throw new EntityAlreadyKnownException();
        }

        _knownEntities.Add(entityId, entity);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that appends a single command associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity"/>.</param>
    /// <param name="command">The new command that modifies the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public TransactionBuilder<TEntity> Append(Guid entityId, object command)
    {
        AddGeneralTransactionStep(entityId, command);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that adds a set of <see cref="ILease"/>s associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity"/>.</param>
    /// <param name="leases">The leases to be added to the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public TransactionBuilder<TEntity> Add(Guid entityId, params ILease[] leases)
    {
        AddLeaseTransactionStep(entityId, Array.Empty<ILease>(), leases);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that adds a set of <see cref="ITag"/>s associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity"/>.</param>
    /// <param name="tags">The tags to be added to the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public TransactionBuilder<TEntity> Add(Guid entityId, params ITag[] tags)
    {
        AddTagTransactionStep(entityId, Array.Empty<ITag>(), tags);

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that deletes a set of <see cref="ILease"/>s associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity"/>.</param>
    /// <param name="leases">The leases to be deleted from the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public TransactionBuilder<TEntity> Delete(Guid entityId, params ILease[] leases)
    {
        AddLeaseTransactionStep(entityId, leases, Array.Empty<ILease>());

        return this;
    }

    /// <summary>
    ///     Adds a transaction step that deletes a set of <see cref="ITag"/>s associated with a given entity id.
    /// </summary>
    /// <param name="entityId">The id associated with the <typeparamref name="TEntity"/>.</param>
    /// <param name="tags">The tags to be deleted from the <typeparamref name="TEntity"/>.</param>
    /// <returns>The transaction builder.</returns>
    public TransactionBuilder<TEntity> Delete(Guid entityId, params ITag[] tags)
    {
        AddTagTransactionStep(entityId, tags, Array.Empty<ITag>());

        return this;
    }

    /// <summary>
    ///     Returns a new instance of <see cref="ITransaction{TEntity}" />.
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="transactionId">A new id for the new transaction.</param>
    /// <returns>A new instance of <see cref="ITransaction{TEntity}" />.</returns>
    public ITransaction<TEntity> Build(string agentSignatureOptionsName, Guid transactionId)
    {
        var agent = _agentAccessor.GetAgent();

        var transaction = new Transaction<TEntity>
        {
            Id = transactionId,
            TimeStamp = agent.GetTimestamp(),
            AgentSignature = agent.GetSignature(agentSignatureOptionsName),
            Steps = _transactionSteps.ToImmutableArray()
        };

        _transactionSteps.Clear();

        return transaction;
    }
}
