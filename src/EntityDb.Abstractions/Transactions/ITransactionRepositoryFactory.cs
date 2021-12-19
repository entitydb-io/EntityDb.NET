using System.Threading.Tasks;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    ///     Represents a type used to create instances of <see cref="ITransactionRepository{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity stored by the <see cref="ITransactionRepository{TEntity}" />.</typeparam>
    public interface ITransactionRepositoryFactory<TEntity>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ITransactionRepository{TEntity}" />.
        /// </summary>
        /// <param name="transactionSessionOptionsName">The agent's use case for the repository.</param>
        /// <returns>A new instance of <see cref="ITransactionRepository{TEntity}" />.</returns>
        Task<ITransactionRepository<TEntity>> CreateRepository(string transactionSessionOptionsName);
    }
}
