using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Common.Queries.SortBuilders;

internal sealed record TagReverseSortBuilder<TSort>
    (ITagSortBuilder<TSort> TagSortBuilder) : ReverseSortBuilderBase<TSort>(TagSortBuilder), ITagSortBuilder<TSort>
{
    public TSort EntityId(bool ascending)
    {
        return TagSortBuilder.EntityId(!ascending);
    }

    public TSort EntityVersionNumber(bool ascending)
    {
        return TagSortBuilder.EntityVersionNumber(!ascending);
    }

    public TSort TagType(bool ascending)
    {
        return TagSortBuilder.TagType(!ascending);
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
