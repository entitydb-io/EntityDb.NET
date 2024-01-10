using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedMessageDataDataQuery : ModifiedQueryBase, IMessageDataDataQuery
{
    public required IMessageDataDataQuery MessageDataDataQuery { get; init; }
    protected override IDataQuery DataQuery => MessageDataDataQuery;

    public TFilter GetFilter<TFilter>(IMessageDataFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                MessageDataDataQuery.GetFilter(builder)
            );
        }

        return MessageDataDataQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(IMessageDataSortBuilder<TSort> builder)
    {
        return MessageDataDataQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
