using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.SqlDb.Documents.Command;
using EntityDb.SqlDb.Queries.Definitions.Sort;

namespace EntityDb.SqlDb.Queries.SortBuilders;

internal sealed class CommandSortBuilder : SortBuilderBase, ICommandSortBuilder<ISortDefinition>
{
    public ISortDefinition EntityId(bool ascending)
    {
        return Sort(ascending, nameof(CommandDocument.EntityId));
    }

    public ISortDefinition EntityVersionNumber(bool ascending)
    {
        return Sort(ascending, nameof(CommandDocument.EntityVersionNumber));
    }

    public ISortDefinition CommandType(bool ascending)
    {
        return SortDataType(ascending);
    }
}
