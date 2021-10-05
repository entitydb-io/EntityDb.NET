using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions
{
    /// <summary>
    ///     Provides a way to construct an <see cref="ITransaction{TEntity}" />. Note that no operations are permanent until
    ///     you call <see cref="Build(Guid, object, DateTime?)" /> and pass the result to a transaction repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the transaction.</typeparam>
    public sealed class TransactionBuilder<TEntity>
    {
        private readonly Dictionary<Guid, TEntity> _knownEntities = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly List<TransactionCommand<TEntity>> _transactionCommands = new();

        /// <summary>
        ///     Initializes a new instance of <see cref="TransactionBuilder{TEntity}" />.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public TransactionBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private static ImmutableArray<ITransactionFact<TEntity>> GetTransactionFacts(IEnumerable<IFact<TEntity>> facts)
        {
            var transactionFacts = new List<TransactionFact<TEntity>>();

            ulong subversionNumber = default;

            foreach (var fact in facts)
            {
                transactionFacts.Add(new TransactionFact<TEntity> { SubversionNumber = subversionNumber, Fact = fact });

                subversionNumber += 1;
            }

            return transactionFacts.ToImmutableArray<ITransactionFact<TEntity>>();
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

        private void AddTransactionCommand(Guid entityId, ICommand<TEntity> command)
        {
            var previousEntity = _knownEntities[entityId];
            var previousVersionNumber = _serviceProvider.GetVersionNumber(previousEntity);

            CommandNotAuthorizedException.ThrowIfFalse(_serviceProvider.IsAuthorized(previousEntity, command));

            var nextFacts = previousEntity.Execute(command);

            nextFacts.Add(_serviceProvider.GetVersionNumberFact<TEntity>(previousVersionNumber + 1));

            var nextEntity = previousEntity.Reduce(nextFacts);

            _transactionCommands.Add(new TransactionCommand<TEntity>
            {
                PreviousSnapshot = previousEntity,
                NextSnapshot = nextEntity,
                EntityId = entityId,
                ExpectedPreviousVersionNumber = previousVersionNumber,
                Command = command,
                Facts = GetTransactionFacts(nextFacts),
                Leases = GetTransactionMetaData(previousEntity, nextEntity, _serviceProvider.GetLeases),
                Tags = GetTransactionMetaData(previousEntity, nextEntity, _serviceProvider.GetTags)
            });

            _knownEntities[entityId] = nextEntity;
        }

        /// <summary>
        ///     Loads an already-existing entity into the builder.
        /// </summary>
        /// <param name="entityId">The id of the entity to load.</param>
        /// <param name="entityRepository">The repository which encapsulates transactions and snapshots.</param>
        /// <returns>A new task that loads an already-existing entity into the builder.</returns>
        /// <remarks>
        ///     Call this method to load an entity that already exists before calling
        ///     <see cref="Append(Guid, ICommand{TEntity})" />.
        /// </remarks>
        public async Task Load(Guid entityId, IEntityRepository<TEntity> entityRepository)
        {
            if (_knownEntities.ContainsKey(entityId))
            {
                throw new EntityAlreadyLoadedException();
            }

            var entity = await entityRepository.GetCurrentOrConstruct(entityId);

            if (_serviceProvider.GetVersionNumber(entity) == 0)
            {
                throw new EntityNotCreatedException();
            }

            _knownEntities.Add(entityId, entity);
        }

        /// <summary>
        ///     Adds a transaction step that creates a new entity.
        /// </summary>
        /// <param name="entityId">A new id for the new entity.</param>
        /// <param name="command">The very first command for the new entity.</param>
        /// <returns>The transaction builder.</returns>
        /// <remarks>
        ///     Do not call this method for an entity that already exists.
        /// </remarks>
        public TransactionBuilder<TEntity> Create(Guid entityId, ICommand<TEntity> command)
        {
            if (_knownEntities.ContainsKey(entityId))
            {
                throw new EntityAlreadyCreatedException();
            }

            var entity = _serviceProvider.Construct<TEntity>(entityId);

            _knownEntities.Add(entityId, entity);

            AddTransactionCommand(entityId, command);

            return this;
        }

        /// <summary>
        ///     Adds a transaction step that appends to an that has already been created.
        /// </summary>
        /// <param name="entityId">The id for the existing entity.</param>
        /// <param name="command">A new command for the existing entity.</param>
        /// <returns>The transaction builder.</returns>
        public TransactionBuilder<TEntity> Append(Guid entityId, ICommand<TEntity> command)
        {
            if (_knownEntities.ContainsKey(entityId) == false)
            {
                throw new EntityNotLoadedException();
            }

            AddTransactionCommand(entityId, command);

            return this;
        }

        /// <summary>
        ///     Returns a new instance of <see cref="ITransaction{TEntity}" />.
        /// </summary>
        /// <param name="transactionId">A new id for the new transaction.</param>
        /// <param name="source">A description of the agent who has requested this transaction.</param>
        /// <param name="timeStampOverride">
        ///     An optional override for the transaction timestamp. The default is
        ///     <see cref="DateTime.UtcNow" />.
        /// </param>
        /// <returns>A new instance of <see cref="ITransaction{TEntity}" />.</returns>
        public ITransaction<TEntity> Build(Guid transactionId, object source, DateTime? timeStampOverride = null)
        {
            var timeStamp = DateTime.UtcNow;

            if (timeStampOverride.HasValue)
            {
                timeStamp = timeStampOverride.Value.ToUniversalTime();
            }

            var transaction = new Transaction<TEntity>
            {
                Id = transactionId,
                TimeStamp = timeStamp,
                Source = source,
                Commands = _transactionCommands.ToImmutableArray<ITransactionCommand<TEntity>>()
            };

            _transactionCommands.Clear();

            return transaction;
        }
    }
}
