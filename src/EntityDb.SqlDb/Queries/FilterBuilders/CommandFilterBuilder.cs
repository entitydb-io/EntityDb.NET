using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.SqlDb.Documents.Command;
using EntityDb.SqlDb.Queries.Definitions.Filter;
using System;

namespace EntityDb.SqlDb.Queries.FilterBuilders;

internal sealed class CommandFilterBuilder : FilterBuilderBase, ICommandFilterBuilder<IFilterDefinition>
{
    public IFilterDefinition EntityIdIn(params Id[] entityIds)
    {
        return In(nameof(CommandDocument.EntityId), entityIds);
    }

    public IFilterDefinition EntityVersionNumberGte(VersionNumber entityVersionNumber)
    {
        return Gte(nameof(CommandDocument.EntityVersionNumber), entityVersionNumber);
    }

    public IFilterDefinition EntityVersionNumberLte(VersionNumber entityVersionNumber)
    {
        return Lte(nameof(CommandDocument.EntityVersionNumber), entityVersionNumber);
    }

    public IFilterDefinition CommandTypeIn(params Type[] commandTypes)
    {
        return DataTypeIn(commandTypes);
    }
}
