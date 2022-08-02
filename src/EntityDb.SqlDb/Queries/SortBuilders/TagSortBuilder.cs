using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.SqlDb.Documents.Tag;
using EntityDb.SqlDb.Queries.Definitions.Sort;

namespace EntityDb.SqlDb.Queries.SortBuilders;

internal sealed class TagSortBuilder : SortBuilderBase, ITagSortBuilder<ISortDefinition>
{
    public ISortDefinition EntityId(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.EntityId));
    }

    public ISortDefinition EntityVersionNumber(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.EntityVersionNumber));
    }

    public ISortDefinition TagType(bool ascending)
    {
        return SortDataType(ascending);
    }

    public ISortDefinition TagLabel(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.Label));
    }

    public ISortDefinition TagValue(bool ascending)
    {
        return Sort(ascending, nameof(TagDocument.Value));
    }
}
