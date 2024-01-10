using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseSourceDataSortBuilder<TSort> : ReverseSortBuilderBase<TSort>,
    ISourceDataSortBuilder<TSort>
{
    public required ISourceDataSortBuilder<TSort> SourceDataSortBuilder { get; init; }
    protected override ISortBuilder<TSort> SortBuilder => SourceDataSortBuilder;

    public TSort StateIds(bool ascending)
    {
        return SourceDataSortBuilder.StateIds(!ascending);
    }
}
