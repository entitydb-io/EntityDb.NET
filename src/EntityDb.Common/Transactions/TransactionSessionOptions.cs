using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Transactions
{
    /// <inheritdoc cref="ITransactionSessionOptions" />
    public sealed record TransactionSessionOptions : ITransactionSessionOptions
    {
        /// <inheritdoc />
        public bool ReadOnly { get; init; }

        /// <inheritdoc />
        public bool SecondaryPreferred { get; init; }

        /// <inheritdoc />
        public TimeSpan? WriteTimeout { get; init; }

        /// <inheritdoc />
        public ILogger? LoggerOverride { get; init; }
    }
}
