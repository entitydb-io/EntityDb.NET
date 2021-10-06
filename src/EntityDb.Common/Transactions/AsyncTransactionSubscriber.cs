using EntityDb.Abstractions.Transactions;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions
{
    /// <summary>
    ///     Represents an asynchronous subscription to transactions.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class AsyncTransactionSubscriber<TEntity> : ITransactionSubscriber<TEntity>
    {
        private readonly bool _testMode;

        /// <summary>
        ///     Constructs a new instance of <see cref="AsyncTransactionSubscriber{TEntity}" />.
        /// </summary>
        /// <param name="testMode">If <c>true</c> then the task will be synchronously awaited before returning.</param>
        protected AsyncTransactionSubscriber(bool testMode)
        {
            _testMode = testMode;
        }

        /// <inheritdoc cref="ITransactionSubscriber{TEntity}.Notify(ITransaction{TEntity})" />
        public void Notify(ITransaction<TEntity> transaction)
        {
            var task = Task.Run(() => NotifyAsync(transaction));

            if (_testMode)
            {
                task.Wait();
            }
        }

        /// <inheritdoc cref="ITransactionSubscriber{TEntity}.Notify(ITransaction{TEntity})"/>
        /// <returns>A task that handles notification asynchronously.</returns>
        protected abstract Task NotifyAsync(ITransaction<TEntity> transaction);
    }
}
