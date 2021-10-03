using EntityDb.Abstractions.Transactions;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions
{
    internal sealed record TransactionMetaData<TMetaData> : ITransactionMetaData<TMetaData>
    {
        public ImmutableArray<TMetaData> Delete { get; init; } = ImmutableArray<TMetaData>.Empty;

        public ImmutableArray<TMetaData> Insert { get; init; } = ImmutableArray<TMetaData>.Empty;
    }
}
