using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.SqlDb.Documents.Lease;
using EntityDb.SqlDb.Queries.Definitions.Filter;

namespace EntityDb.SqlDb.Queries.FilterBuilders;

internal sealed class LeaseFilterBuilder : FilterBuilderBase, ILeaseFilterBuilder<IFilterDefinition>
{
    public IFilterDefinition EntityIdIn(params Id[] entityIds)
    {
        return In(nameof(LeaseDocument.EntityId), entityIds);
    }

    public IFilterDefinition EntityVersionNumberGte(VersionNumber entityVersionNumber)
    {
        return Gte(nameof(LeaseDocument.EntityVersionNumber), entityVersionNumber);
    }

    public IFilterDefinition EntityVersionNumberLte(VersionNumber entityVersionNumber)
    {
        return Lte(nameof(LeaseDocument.EntityVersionNumber), entityVersionNumber);
    }

    public IFilterDefinition LeaseTypeIn(params Type[] leaseTypes)
    {
        return DataTypeIn(leaseTypes);
    }

    public IFilterDefinition LeaseScopeEq(string scope)
    {
        return Eq(nameof(LeaseDocument.Scope), scope);
    }

    public IFilterDefinition LeaseLabelEq(string label)
    {
        return Eq(nameof(LeaseDocument.Label), label);
    }

    public IFilterDefinition LeaseValueEq(string value)
    {
        return Eq(nameof(LeaseDocument.Value), value);
    }
}
