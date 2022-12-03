using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Common.Queries.SortBuilders;

internal sealed record CommandReverseSortBuilder<TSort>
    (ICommandSortBuilder<TSort> CommandSortBuilder) : ReverseSortBuilderBase<TSort>(CommandSortBuilder),
        ICommandSortBuilder<TSort>
{
    public TSort EntityId(bool ascending)
    {
        return CommandSortBuilder.EntityId(!ascending);
    }

    public TSort EntityVersionNumber(bool ascending)
    {
        return CommandSortBuilder.EntityVersionNumber(!ascending);
    }

    public TSort CommandType(bool ascending)
    {
        return CommandSortBuilder.CommandType(!ascending);
    }
}
