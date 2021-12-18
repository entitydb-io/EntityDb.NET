using EntityDb.Abstractions.Transactions;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions
{
    /// <summary>
    ///     Represents an asynchronous subscription to transactions.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class TransactionSubscriber<TEntity> : ITransactionSubscriber<TEntity>
    {
        private readonly bool _synchronousMode;

        /// <summary>
        ///     Constructs a new instance of <see cref="TransactionSubscriber{TEntity}" />.
        /// </summary>
        /// <param name="synchronousMode">If <c>true</c> then the task will be synchronously awaited before returning.</param>
        protected TransactionSubscriber(bool synchronousMode)
        {
            _synchronousMode = synchronousMode;
        }

        /// <inheritdoc cref="ITransactionSubscriber{TEntity}.Notify(ITransaction{TEntity})" />
        public void Notify(ITransaction<TEntity> transaction)
        {
            var task = Task.Run(async () => await NotifyAsync(transaction).ConfigureAwait(false));

            if (_synchronousMode)
            {
                task.Wait();
            }
        }

        /// <inheritdoc cref="ITransactionSubscriber{TEntity}.Notify(ITransaction{TEntity})"/>
        /// <returns>A task that handles notification asynchronously.</returns>
        protected abstract Task NotifyAsync(ITransaction<TEntity> transaction);
    }
}
