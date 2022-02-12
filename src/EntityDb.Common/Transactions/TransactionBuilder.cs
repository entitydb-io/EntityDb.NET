using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace EntityDb.Common.Transactions
{
    /// <summary>
    ///     Provides a way to construct an <see cref="ITransaction{TEntity}" />. Note that no operations are permanent until
    ///     you call <see cref="Build(string, Guid)" /> and pass the result to a transaction repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the transaction.</typeparam>
    public sealed class TransactionBuilder<TEntity>
    {
        private readonly Dictionary<Guid, TEntity> _knownEntities = new();
        private readonly List<TransactionStep<TEntity>> _transactionSteps = new();

        private readonly IAgentAccessor _agentAccessor;
        private readonly IConstructingStrategy<TEntity> _constructingStrategy;
        private readonly IVersioningStrategy<TEntity> _versioningStrategy;
        private readonly IAuthorizingStrategy<TEntity>? _authorizingStrategy;
        private readonly ILeasingStrategy<TEntity>? _leasingStrategy;
        private readonly ITaggingStrategy<TEntity>? _taggingStrategy;

        /// <summary>
        ///     Initializes a new instance of <see cref="TransactionBuilder{TEntity}" />.
        /// </summary>
        public TransactionBuilder
        (
            IAgentAccessor agentAccessor,
            IConstructingStrategy<TEntity> constructingStrategy,
            IVersioningStrategy<TEntity> versioningStrategy,
            IAuthorizingStrategy<TEntity>? authorizingStrategy = null,
            ILeasingStrategy<TEntity>? leasingStrategy = null,
            ITaggingStrategy<TEntity>? taggingStrategy = null
        )
        {
            _agentAccessor = agentAccessor;
            _constructingStrategy = constructingStrategy;
            _versioningStrategy = versioningStrategy;
            _authorizingStrategy = authorizingStrategy;
            _leasingStrategy = leasingStrategy;
            _taggingStrategy = taggingStrategy;
        }

        private static ITransactionMetaData<TMetaData> GetTransactionMetaData<TMetaData>(TEntity previousEntity,
            TEntity nextEntity, Func<TEntity, TMetaData[]> metaDataMapper)
        {
            var previousMetaData = metaDataMapper.Invoke(previousEntity);
            var nextMetaData = metaDataMapper.Invoke(nextEntity);

            return new TransactionMetaData<TMetaData>
            {
                Delete = previousMetaData.Except(nextMetaData).ToImmutableArray(),
                Insert = nextMetaData.Except(previousMetaData).ToImmutableArray()
            };
        }

        private bool IsAuthorized(TEntity entity, ICommand<TEntity> command)
        {
            return _authorizingStrategy?.IsAuthorized(entity, command) ?? true;
        }

        private ILease[] GetLeases(TEntity entity)
        {
            return _leasingStrategy?.GetLeases(entity) ?? Array.Empty<ILease>();
        }

        private ITag[] GetTags(TEntity entity)
        {
            return _taggingStrategy?.GetTags(entity) ?? Array.Empty<ITag>();
        }

        private void AddTransactionStep(Guid entityId, ICommand<TEntity> command)
        {
            var previousEntity = _knownEntities[entityId];
            var previousEntityVersionNumber = _versioningStrategy.GetVersionNumber(previousEntity);

            CommandNotAuthorizedException.ThrowIfFalse(IsAuthorized(previousEntity, command));

            var nextEntity = previousEntity.Reduce(command);
            var nextEntityVersionNumber = _versioningStrategy.GetVersionNumber(nextEntity);

            _transactionSteps.Add(new TransactionStep<TEntity>
            {
                PreviousEntitySnapshot = previousEntity,
                PreviousEntityVersionNumber = previousEntityVersionNumber,
                NextEntitySnapshot = nextEntity,
                NextEntityVersionNumber = nextEntityVersionNumber,
                EntityId = entityId,
                Command = command,
                Leases = GetTransactionMetaData(previousEntity, nextEntity, GetLeases),
                Tags = GetTransactionMetaData(previousEntity, nextEntity, GetTags)
            });

            _knownEntities[entityId] = nextEntity;
        }

        /// <summary>
        ///     Returns a single-entity transaction builder, which has a simplified set of methods.
        /// </summary>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns>A single-entity transaction builder, which has a simplified set of methods.</returns>
        public SingleEntityTransactionBuilder<TEntity> ForSingleEntity(Guid entityId)
        {
            return new SingleEntityTransactionBuilder<TEntity>(entityId, this);
        }

        /// <summary>
        ///     Returns a <typeparamref name="TEntity"/> associated with a given entity id, if it is known.
        /// </summary>
        /// <param name="entityId">The id assocaited with the entity.</param>
        /// <returns>A <typeparamref name="TEntity"/> associated with <paramref name="entityId"/>, if it is known.</returns>
        public TEntity GetEntity(Guid entityId)
        {
            return _knownEntities[entityId];
        }

        /// <summary>
        ///     Indicates wether or not a <typeparamref name="TEntity"/> associated with a given entity id is in memory (i.e., created or loaded).
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
        ///     <see cref="Append(Guid, ICommand{TEntity})" />.
        /// </remarks>
        public TransactionBuilder<TEntity> Load(Guid entityId, TEntity entity)
        {
            if (IsEntityKnown(entityId))
            {
                throw new EntityAlreadyLoadedException();
            }

            _knownEntities.Add(entityId, entity);

            return this;
        }

        /// <summary>
        ///     Adds a transaction step that creates a new <typeparamref name="TEntity"/> associated with a given entity id.
        /// </summary>
        /// <param name="entityId">A new id associated with the new <typeparamref name="TEntity"/>.</param>
        /// <param name="command">The very first command for the new <typeparamref name="TEntity"/>.</param>
        /// <returns>The transaction builder.</returns>
        /// <remarks>
        ///     Do not call this method for a <typeparamref name="TEntity"/> that already exists.
        /// </remarks>
        public TransactionBuilder<TEntity> Create(Guid entityId, ICommand<TEntity> command)
        {
            if (IsEntityKnown(entityId))
            {
                throw new EntityAlreadyCreatedException();
            }

            var entity = _constructingStrategy.Construct(entityId);

            _knownEntities.Add(entityId, entity);

            AddTransactionStep(entityId, command);

            return this;
        }

        /// <summary>
        ///     Adds a transaction step that appends to an existing <typeparamref name="TEntity"/> associated with a given entity id.
        /// </summary>
        /// <param name="entityId">An existing id associated with an existing <typeparamref name="TEntity"/>.</param>
        /// <param name="command">A new command for the existing <typeparamref name="TEntity"/>.</param>
        /// <returns>The transaction builder.</returns>
        public TransactionBuilder<TEntity> Append(Guid entityId, ICommand<TEntity> command)
        {
            if (!IsEntityKnown(entityId))
            {
                throw new EntityNotLoadedException();
            }

            AddTransactionStep(entityId, command);

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
                Steps = _transactionSteps.ToImmutableArray<ITransactionStep<TEntity>>()
            };

            _transactionSteps.Clear();

            return transaction;
        }
    }
}
