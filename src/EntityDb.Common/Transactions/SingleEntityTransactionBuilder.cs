using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Transactions
{
    /// <summary>
    ///     A transaction builder for a single entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class SingleEntityTransactionBuilder<TEntity>
    {
        private readonly Guid _entityId;
        private readonly TransactionBuilder<TEntity> _transactionBuilder;

        internal SingleEntityTransactionBuilder(Guid entityId, TransactionBuilder<TEntity> transactionBuilder)
        {
            _entityId = entityId;
            _transactionBuilder = transactionBuilder;
        }

        /// <summary>
        ///     The id used for all transaction builder methods, where applicable.
        /// </summary>
        public Guid EntityId => _entityId;

        /// <summary>
        ///     Returns a <typeparamref name="TEntity"/>, if it is known.
        /// </summary>
        /// <returns>A <typeparamref name="TEntity"/>, if it is known.</returns>
        public TEntity GetEntity()
        {
            return _transactionBuilder.GetEntity(_entityId);
        }

        /// <summary>
        ///     Indicates wether or not a <typeparamref name="TEntity"/> is in memory (i.e., created or loaded).
        /// </summary>
        /// <returns><c>true</c> if a <typeparamref name="TEntity"/> is in memory, or else <c>false</c>.</returns>
        public bool IsEntityKnown()
        {
            return _transactionBuilder.IsEntityKnown(_entityId);
        }


        /// <summary>
        ///     Associate a <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="entity">A <typeparamref name="TEntity"/></param>
        /// <returns>The transaction builder.</returns>
        /// <remarks>
        ///     Call this method to load an entity that already exists before calling
        ///     <see cref="Append(ICommand{TEntity})"/>.
        /// </remarks>
        public SingleEntityTransactionBuilder<TEntity> Load(TEntity entity)
        {
            _transactionBuilder.Load(_entityId, entity);

            return this;
        }

        /// <summary>
        ///    Adds a transaction step that creates a new <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="command">The very first command for the new <typeparamref name="TEntity"/>.</param>
        /// <returns>The transaction builder.</returns>
        /// <remarks>
        ///     Do not call this method for a <typeparamref name="TEntity"/> that already exists.
        /// </remarks>
        public SingleEntityTransactionBuilder<TEntity> Create(ICommand<TEntity> command)
        {
            _transactionBuilder.Create(_entityId, command);

            return this;
        }

        /// <summary>
        ///     Adds a transaction step that appends to an existing <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="command">A new command for the existing <typeparamref name="TEntity"/>.</param>
        /// <returns>The transaction builder.</returns>
        public SingleEntityTransactionBuilder<TEntity> Append(ICommand<TEntity> command)
        {
            _transactionBuilder.Append(_entityId, command);

            return this;
        }

        /// <summary>
        ///     Returns a new instance of <see cref="ITransaction{TEntity}" />.
        /// </summary>
        /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
        /// <param name="transactionId">A new id for the new transaction.</param>
        /// <returns>A new instance of <see cref="ITransaction{TEntity}" />.</returns>
        /// <remarks>
        ///     Note that this is just a proxy for a <see cref="TransactionBuilder{TEntity}"/>,
        ///     and does NOT filter out steps for entity ids not associated with this
        ///     <see cref="SingleEntityTransactionBuilder{TEntity}"/>.
        /// </remarks>
        public ITransaction<TEntity> Build(string agentSignatureOptionsName, Guid transactionId)
        {
            return _transactionBuilder.Build(agentSignatureOptionsName, transactionId);
        }
    }
}
