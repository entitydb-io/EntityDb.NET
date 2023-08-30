using EntityDb.Abstractions.Transactions;

namespace EntityDb.Abstractions.Sources;

/// <summary>
///     Represents sources of information.
/// </summary>
public interface ISourceRepository
{
    /// <summary>
    ///     The backing transaction repository.
    /// </summary>
    ITransactionRepository TransactionRepository { get; }
}
