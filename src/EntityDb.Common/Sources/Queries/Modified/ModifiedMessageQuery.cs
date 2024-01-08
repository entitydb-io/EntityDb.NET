using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Sources.Queries.Modified;

internal sealed record ModifiedMessageQuery
    (IMessageQuery MessageQuery, ModifiedQueryOptions ModifiedQueryOptions) : ModifiedQueryBase(MessageQuery,
        ModifiedQueryOptions), IMessageQuery
{
    public TFilter GetFilter<TFilter>(IMessageFilterBuilder<TFilter> builder)
    {
        if (ModifiedQueryOptions.InvertFilter)
        {
            return builder.Not
            (
                MessageQuery.GetFilter(builder)
            );
        }

        return MessageQuery.GetFilter(builder);
    }

    public TSort? GetSort<TSort>(IMessageSortBuilder<TSort> builder)
    {
        return MessageQuery.GetSort(ModifiedQueryOptions.ReverseSort
            ? builder.Reverse()
            : builder);
    }
}
