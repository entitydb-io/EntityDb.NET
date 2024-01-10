using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseTagSortBuilder<TSort> : ReverseSortBuilderBase<TSort>, ITagSortBuilder<TSort>
{
    public required ITagSortBuilder<TSort> TagSortBuilder { get; init; }
    protected override ISortBuilder<TSort> SortBuilder => TagSortBuilder;

    public TSort StateId(bool ascending)
    {
        return TagSortBuilder.StateId(!ascending);
    }

    public TSort StateVersion(bool ascending)
    {
        return TagSortBuilder.StateVersion(!ascending);
    }

    public TSort TagLabel(bool ascending)
    {
        return TagSortBuilder.TagLabel(!ascending);
    }

    public TSort TagValue(bool ascending)
    {
        return TagSortBuilder.TagValue(!ascending);
    }
}
