using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Queries;

internal sealed record GetTransactionCommandsQuery(Id TransactionId) : IAgentSignatureQuery, ICommandQuery
{
    public TFilter GetFilter<TFilter>(IAgentSignatureFilterBuilder<TFilter> builder)
    {
        return builder.SourceIdIn(TransactionId);
    }

    public TSort? GetSort<TSort>(IAgentSignatureSortBuilder<TSort> builder)
    {
        return default;
    }

    public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
    {
        return builder.SourceIdIn(TransactionId);
    }

    public TSort? GetSort<TSort>(ICommandSortBuilder<TSort> builder)
    {
        return default;
    }

    public int? Skip => default;

    public int? Take => default;

    public object? Options => default;
}
