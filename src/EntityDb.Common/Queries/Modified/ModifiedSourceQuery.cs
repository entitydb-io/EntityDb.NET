using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified
{
    internal sealed record ModifiedSourceQuery(ISourceQuery SourceQuery, bool InvertFilter, bool ReverseSort, int? ReplaceSkip, int? ReplaceTake) : ModifiedQueryBase(SourceQuery, ReplaceSkip, ReplaceTake), ISourceQuery
    {
        public TFilter GetFilter<TFilter>(ISourceFilterBuilder<TFilter> builder)
        {
            if (InvertFilter)
            {
                return builder.Not
                (
                    SourceQuery.GetFilter(builder)
                );
            }

            return SourceQuery.GetFilter(builder);
        }

        public TSort? GetSort<TSort>(ISourceSortBuilder<TSort> builder)
        {
            if (ReverseSort)
            {
                return SourceQuery.GetSort(builder.Reverse());
            }

            return SourceQuery.GetSort(builder);
        }
    }
}
