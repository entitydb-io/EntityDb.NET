using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.Filters;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Common.Queries.Filtered
{
    internal sealed record FilteredCommandQuery
        (ICommandQuery CommandQuery, ICommandFilter CommandFilter) : FilteredQueryBase(CommandQuery), ICommandQuery
    {
        public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                CommandQuery.GetFilter(builder),
                CommandFilter.GetFilter(builder)
            );
        }

        public TSort? GetSort<TSort>(ICommandSortBuilder<TSort> builder)
        {
            return CommandQuery.GetSort(builder);
        }
    }
}
