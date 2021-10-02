using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Transactions
{
    /// <inheritdoc cref="ITransactionSessionOptions"/>
    public sealed class TransactionSessionOptions : ITransactionSessionOptions
    {
        /// <inheritdoc/>
        public bool ReadOnly { get; set; }

        /// <inheritdoc/>
        public bool SecondaryPreferred { get; set; }

        /// <inheritdoc/>
        public TimeSpan? WriteTimeout { get; set; }

        /// <inheritdoc/>
        public ILogger? LoggerOverride { get; set; }
    }
}
