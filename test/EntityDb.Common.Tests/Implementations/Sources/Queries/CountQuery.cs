using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Tests.Implementations.States.Attributes;

namespace EntityDb.Common.Tests.Implementations.Sources.Queries;

public record CountQuery(ulong Gte, ulong Lte, object? Options = null) : ILeaseQuery, ITagQuery
{
    public int? Skip => null;

    public int? Take => null;

    public TFilter GetFilter<TFilter>(ILeaseDataFilterBuilder<TFilter> builder)
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

    public TFilter GetFilter<TFilter>(ITagDataFilterBuilder<TFilter> builder)
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
