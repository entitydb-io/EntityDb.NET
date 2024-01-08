using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseMessageSortBuilder<TSort>
    (IMessageSortBuilder<TSort> MessageSortBuilder) : ReverseSortBuilderBase<TSort>(MessageSortBuilder),
        IMessageSortBuilder<TSort>
{
    public TSort EntityId(bool ascending)
    {
        return MessageSortBuilder.EntityId(!ascending);
    }

    public TSort EntityVersion(bool ascending)
    {
        return MessageSortBuilder.EntityVersion(!ascending);
    }
}
