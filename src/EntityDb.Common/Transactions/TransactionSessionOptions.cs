using EntityDb.Abstractions.Loggers;
using System;

namespace EntityDb.Common.Transactions
{
    /// <summary>
    ///     Represents the agent's use case for the transaction repository.
    /// </summary>
    public sealed class TransactionSessionOptions
    {
        /// <summary>
        ///     If <c>true</c>, indicates the agent only intends to execute queries.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        ///     If <c>true</c>, indicates the agent can tolerate replication lag for queries.
        /// </summary>
        public bool SecondaryPreferred { get; set; }

        /// <summary>
        ///     Determines how long to wait before a command should be automatically aborted.
        /// </summary>
        public TimeSpan? WriteTimeout { get; set; }

        /// <summary>
        ///     Determines how long to wait before a query should be automatically killed.
        /// </summary>
        public TimeSpan? ReadTimeout { get; set; }

        /// <summary>
        ///     Overrides the logger for the session.
        /// </summary>
        public ILogger? LoggerOverride { get; set; }
    }
}
