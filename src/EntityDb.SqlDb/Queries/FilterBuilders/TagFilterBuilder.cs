using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.SqlDb.Documents.Tag;
using EntityDb.SqlDb.Queries.Definitions.Filter;
using System;

namespace EntityDb.SqlDb.Queries.FilterBuilders;

internal sealed class TagFilterBuilder : FilterBuilderBase, ITagFilterBuilder<IFilterDefinition>
{
    public IFilterDefinition EntityIdIn(params Id[] entityIds)
    {
        return In(nameof(TagDocument.EntityId), entityIds);
    }

    public IFilterDefinition EntityVersionNumberGte(VersionNumber entityVersionNumber)
    {
        return Gte(nameof(TagDocument.EntityVersionNumber), entityVersionNumber);
    }

    public IFilterDefinition EntityVersionNumberLte(VersionNumber entityVersionNumber)
    {
        return Lte(nameof(TagDocument.EntityVersionNumber), entityVersionNumber);
    }

    public IFilterDefinition TagTypeIn(params Type[] tagTypes)
    {
        return DataTypeIn(tagTypes);
    }

    public IFilterDefinition TagLabelEq(string label)
    {
        return Eq(nameof(TagDocument.Label), label);
    }

    public IFilterDefinition TagValueEq(string value)
    {
        return Eq(nameof(TagDocument.Value), value);
    }
}
