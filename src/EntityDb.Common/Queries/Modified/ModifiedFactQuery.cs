using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified
{
    internal sealed record ModifiedFactQuery(IFactQuery FactQuery, ModifiedQueryOptions ModifiedQueryOptions) : ModifiedQueryBase(FactQuery, ModifiedQueryOptions), IFactQuery
    {
        public TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder)
        {
            if (ModifiedQueryOptions.InvertFilter)
            {
                return builder.Not
                (
                    FactQuery.GetFilter(builder)
                );
            }

            return FactQuery.GetFilter(builder);
        }

        public TSort? GetSort<TSort>(IFactSortBuilder<TSort> builder)
        {
            if (ModifiedQueryOptions.ReverseSort)
            {
                return FactQuery.GetSort(builder.Reverse());
            }

            return FactQuery.GetSort(builder);
        }
    }
}
