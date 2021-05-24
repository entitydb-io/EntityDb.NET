using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified
{
    internal sealed record ModifiedCommandQuery(ICommandQuery CommandQuery, bool InvertFilter, bool ReverseSort, int? ReplaceSkip, int? ReplaceTake) : ModifiedQueryBase(CommandQuery, ReplaceSkip, ReplaceTake), ICommandQuery
    {
        public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            if (InvertFilter)
            {
                return builder.Not
                (
                    CommandQuery.GetFilter(builder)
                );
            }

            return CommandQuery.GetFilter(builder);
        }

        public TSort? GetSort<TSort>(ICommandSortBuilder<TSort> builder)
        {
            if (ReverseSort)
            {
                return CommandQuery.GetSort(builder.Reverse());
            }

            return CommandQuery.GetSort(builder);
        }
    }
}
