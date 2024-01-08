using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Tags;

namespace EntityDb.Common.Tests.Implementations.Queries;

public record CountQuery(ulong Gte, ulong Lte, object? Options = null) : ILeaseQuery, ITagQuery
{
    public int? Skip => null;

    public int? Take => null;

    public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.DataTypeIn(typeof(CountLease)),
            builder.Or
            (
                Enumerable
                    .Range((int)Gte, (int)(Lte - Gte + 1))
                    .Select(number => builder.LeaseValueEq(number.ToString()))
                    .ToArray()
            )
        );
    }

    public TSort GetSort<TSort>(ILeaseSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.LeaseValue(true)
        );
    }

    public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.DataTypeIn(typeof(CountTag)),
            builder.Or
            (
                Enumerable
                    .Range((int)Gte, (int)(Lte - Gte + 1))
                    .Select(number => builder.TagValueEq(number.ToString()))
                    .ToArray()
            )
        );
    }

    public TSort GetSort<TSort>(ITagSortBuilder<TSort> builder)
    {
        return builder.Combine
        (
            builder.TagValue(true)
        );
    }
}