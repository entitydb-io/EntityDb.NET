using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseTagDataSortBuilder<TSort> : ReverseSortBuilderBase<TSort>, ITagDataSortBuilder<TSort>
{
    public required ITagDataSortBuilder<TSort> TagDataSortBuilder { get; init; }
    protected override IDataSortBuilder<TSort> DataSortBuilder => TagDataSortBuilder;

    public TSort StateId(bool ascending)
    {
        return TagDataSortBuilder.StateId(!ascending);
    }

    public TSort StateVersion(bool ascending)
    {
        return TagDataSortBuilder.StateVersion(!ascending);
    }

    public TSort TagLabel(bool ascending)
    {
        return TagDataSortBuilder.TagLabel(!ascending);
    }

    public TSort TagValue(bool ascending)
    {
        return TagDataSortBuilder.TagValue(!ascending);
    }
}
