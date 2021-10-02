using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified
{
    internal sealed record ModifiedFactQuery(IFactQuery FactQuery, bool InvertFilter, bool ReverseSort, int? ReplaceSkip, int? ReplaceTake) : ModifiedQueryBase(FactQuery, ReplaceSkip, ReplaceTake), IFactQuery
    {
        public TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder)
        {
            if (InvertFilter)
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
            if (ReverseSort)
            {
                return FactQuery.GetSort(builder.Reverse());
            }

            return FactQuery.GetSort(builder);
        }
    }
}
