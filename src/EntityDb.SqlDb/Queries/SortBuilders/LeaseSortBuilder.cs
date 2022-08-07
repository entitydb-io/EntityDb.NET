using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.SqlDb.Documents.Lease;
using EntityDb.SqlDb.Queries.Definitions.Sort;

namespace EntityDb.SqlDb.Queries.SortBuilders;

internal sealed class LeaseSortBuilder : SortBuilderBase, ILeaseSortBuilder<ISortDefinition>
{
    public ISortDefinition EntityId(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.EntityId));
    }

    public ISortDefinition EntityVersionNumber(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.EntityVersionNumber));
    }

    public ISortDefinition LeaseType(bool ascending)
    {
        return SortDataType(ascending);
    }

    public ISortDefinition LeaseScope(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.Scope));
    }

    public ISortDefinition LeaseLabel(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.Label));
    }

    public ISortDefinition LeaseValue(bool ascending)
    {
        return Sort(ascending, nameof(LeaseDocument.Value));
    }
}
