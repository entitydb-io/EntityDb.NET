using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedMessageDataQuery : ModifiedQueryBase, IMessageDataQuery
{
    public required IMessageDataQuery MessageDataQuery { get; init; }
    protected override IDataQuery DataQuery => MessageDataQuery;

    public TFilter GetFilter<TFilter>(IMessageDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                MessageDataQuery.GetFilter(builder)
            );
        }

        return MessageDataQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(IMessageDataSortBuilder<TSort> builder)
    {
        return MessageDataQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
