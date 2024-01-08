using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedMessageGroupQuery
    (IMessageGroupQuery MessageGroupQuery, ModifiedQueryOptions ModifiedQueryOptions) : ModifiedQueryBase(
        MessageGroupQuery,
        ModifiedQueryOptions), IMessageGroupQuery
{
    public TFilter GetFilter<TFilter>(IMessageGroupFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                MessageGroupQuery.GetFilter(builder)
            );
        }

        return MessageGroupQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(IMessageGroupSortBuilder<TSort> builder)
    {
        return MessageGroupQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
