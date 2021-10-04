using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified
{
    internal sealed record ModifiedCommandQuery
        (ICommandQuery CommandQuery, ModifiedQueryOptions ModifiedQueryOptions) : ModifiedQueryBase(CommandQuery,
            ModifiedQueryOptions), ICommandQuery
    {
        public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            if (ModifiedQueryOptions.InvertFilter)
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
            if (ModifiedQueryOptions.ReverseSort)
            {
                return CommandQuery.GetSort(builder.Reverse());
            }

            return CommandQuery.GetSort(builder);
        }
    }
}
