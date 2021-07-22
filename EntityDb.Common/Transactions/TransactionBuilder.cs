using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions
{
    /// <summary>
    /// Provides a way to construct an <see cref="ITransaction{TEntity}"/>. Note that no operations are permanent until you call <see cref="Build(Guid, object, DateTime?)"/> and pass the result to a transaction repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the transaction.</typeparam>
    public sealed class TransactionBuilder<TEntity>
    {
        private readonly Dictionary<Guid, TEntity> _knownEntities = new();
        private readonly List<TransactionCommand<TEntity>> _transactionCommands = new();
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="TransactionBuilder{TEntity}"/>.
        /// </summary>
        /// <param name="claimsPrincipal">The claims of the agent.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public TransactionBuilder(ClaimsPrincipal claimsPrincipal, IServiceProvider serviceProvider)
        {
            _claimsPrincipal = claimsPrincipal;
            _serviceProvider = serviceProvider;
        }

        private void AddTransactionCommand(Guid entityId, ICommand<TEntity> command)
        {
            var previousEntity = _knownEntities[entityId];
            var previousVersionNumber = _serviceProvider.GetVersionNumber(previousEntity);
            var previousLeases = _serviceProvider.GetLeases(previousEntity);

            if (_serviceProvider.IsAuthorized(previousEntity, command, _claimsPrincipal) == false)
            {
                throw new CommandNotAuthorizedException();
            }

            var nextFacts = previousEntity.Execute(command);

            nextFacts.Add(_serviceProvider.GetVersionNumberFact<TEntity>(previousVersionNumber + 1));

            var nextEntity = previousEntity.Reduce(nextFacts);
            var nextLeases = _serviceProvider.GetLeases(nextEntity);

            var transactionFacts = new List<TransactionFact<TEntity>>();

            ulong subversionNumber = default;

            foreach (var nextFact in nextFacts)
            {
                transactionFacts.Add(new TransactionFact<TEntity>(subversionNumber, nextFact));

                subversionNumber += 1;
            }

            _transactionCommands.Add(new TransactionCommand<TEntity>
            (
                entityId,
                previousVersionNumber,
                command,
                transactionFacts.ToArray(),
                previousLeases.Except(nextLeases).ToArray(),
                nextLeases.Except(previousLeases).ToArray()
            ));

            _knownEntities[entityId] = nextEntity;
        }

        /// <summary>
        /// Loads an already-existing entity into the builder.
        /// </summary>
        /// <param name="entityId">The id of the entity to load.</param>
        /// <param name="transactionRepository">The repository which contains relevant entity data.</param>
        /// <param name="snapshotRepository">An optional repository which may or may not contain a snapshot of an entity.</param>
        /// <returns>A new task that loads an already-existing entity into the builder.</returns>
        /// <remarks>
        /// Call this method to load an entity that already exists before calling <see cref="Append(Guid, ICommand{TEntity})"/>.
        /// </remarks>
        public async Task Load(Guid entityId, ITransactionRepository<TEntity> transactionRepository, ISnapshotRepository<TEntity>? snapshotRepository = null)
        {
            if (_knownEntities.ContainsKey(entityId))
            {
                throw new EntityAlreadyLoadedException();
            }

            var entity = await _serviceProvider.GetEntity(entityId, transactionRepository, snapshotRepository);

            if (_serviceProvider.GetVersionNumber(entity) == 0)
            {
                throw new EntityNotCreatedException();
            }

            _knownEntities.Add(entityId, entity);
        }

        /// <summary>
        /// Adds a transaction step that creates a new entity.
        /// </summary>
        /// <param name="entityId">A new id for the new entity.</param>
        /// <param name="command">The very first command for the new entity.</param>
        /// <returns>The transaction builder.</returns>
        /// <remarks>
        /// Do not call this method for an entity that already exists.
        /// </remarks>
        public TransactionBuilder<TEntity> Create(Guid entityId, ICommand<TEntity> command)
        {
            var entity = _serviceProvider.Construct<TEntity>(entityId);

            if (_knownEntities.ContainsKey(entityId))
            {
                throw new EntityAlreadyCreatedException();
            }

            _knownEntities.Add(entityId, entity);

            AddTransactionCommand(entityId, command);

            return this;
        }

        /// <summary>
        /// Adds a transaction step that appends to an that has already been created.
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
        /// Returns a new instance of <see cref="ITransaction{TEntity}"/>.
        /// </summary>
        /// <param name="transactionId">A new id for the new transaction.</param>
        /// <param name="source">A description of the agent who has requested this transaction.</param>
        /// <param name="timeStampOverride">An optional override for the transaction timestamp. The default is <see cref="DateTime.UtcNow"/>.</param>
        /// <returns>A new instance of <see cref="ITransaction{TEntity}"/>.</returns>
        public ITransaction<TEntity> Build(Guid transactionId, object source, DateTime? timeStampOverride = null)
        {
            var timeStamp = DateTime.UtcNow;

            if (timeStampOverride.HasValue)
            {
                timeStamp = timeStampOverride.Value.ToUniversalTime();
            }

            var transaction = new Transaction<TEntity>(transactionId, timeStamp, source, _transactionCommands.ToArray());

            _transactionCommands.Clear();

            return transaction;
        }
    }
}
