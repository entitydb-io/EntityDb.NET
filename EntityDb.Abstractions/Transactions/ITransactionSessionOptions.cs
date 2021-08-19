using EntityDb.Abstractions.Loggers;
using System;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    /// Represents the agent's use case for the transaction repository.
    /// </summary>
    public interface ITransactionSessionOptions
    {
        /// <summary>
        /// If <c>true</c>, indicates the agent only intends to execute queries.
        /// </summary>
        /// <remarks>
        /// <see cref="ITransactionRepository{TEntity}.PutTransaction(ITransaction{TEntity})"/> should always return <c>false</c> if this is <c>true</c>.
        /// </remarks>
        //TODO: Make sure the generic test suite verifies that this requirement is satisified
        bool ReadOnly { get; }

        /// <summary>
        /// If <c>true</c>, indicates the agent can tolerate replication lag for queries.
        /// </summary>
        bool SecondaryPreferred { get; }

        /// <summary>
        /// Determines how long to wait before a command should be automatically aborted.
        /// </summary>
        TimeSpan? WriteTimeout { get; }

        /// <summary>
        /// Overrides the logger for the session.
        /// </summary>
        ILogger? LoggerOverride { get; }
    }
}
