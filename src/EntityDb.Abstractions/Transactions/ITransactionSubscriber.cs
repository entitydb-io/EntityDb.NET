namespace EntityDb.Abstractions.Transactions;

/// <summary>
///     Represents a type that reacts to transactions that have been committed.
/// </summary>
public interface ITransactionSubscriber
{
    /// <summary>
    ///     Called when a transaction has been committed (or on replay, if used in that way).
    /// </summary>
    /// <param name="transaction">The committed transaction.</param>
    void Notify(ITransaction transaction);
}
