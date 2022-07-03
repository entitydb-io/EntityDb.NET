using EntityDb.Abstractions.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Transactions;

/// <summary>
///     Represents a type used to create instances of <see cref="ITransactionRepository" />.
/// </summary>
public interface ITransactionRepositoryFactory : IDisposableResource
{
    /// <summary>
    ///     Creates a new instance of <see cref="ITransactionRepository" />.
    /// </summary>
    /// <param name="transactionSessionOptionsName">The agent's use case for the repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ITransactionRepository" />.</returns>
    Task<ITransactionRepository> CreateRepository(string transactionSessionOptionsName,
        CancellationToken cancellationToken = default);
}
