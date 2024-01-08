using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseMessageGroupSortBuilder<TSort>
    (IMessageGroupSortBuilder<TSort> MessageGroupSortBuilder) : ReverseSortBuilderBase<TSort>(
            MessageGroupSortBuilder),
        IMessageGroupSortBuilder<TSort>
{
    public TSort EntityIds(bool ascending)
    {
        return MessageGroupSortBuilder.EntityIds(!ascending);
    }
}
