using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseTagSortBuilder<TSort>
    (ITagSortBuilder<TSort> TagSortBuilder) : ReverseSortBuilderBase<TSort>(TagSortBuilder), ITagSortBuilder<TSort>
{
    public TSort EntityId(bool ascending)
    {
        return TagSortBuilder.EntityId(!ascending);
    }

    public TSort EntityVersion(bool ascending)
    {
        return TagSortBuilder.EntityVersion(!ascending);
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
