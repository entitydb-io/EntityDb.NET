using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal sealed record ReverseMessageDataSortBuilder<TSort> : ReverseSortBuilderBase<TSort>,
    IMessageDataSortBuilder<TSort>
{
    public required IMessageDataSortBuilder<TSort> MessageDataSortBuilder { get; init; }
    protected override IDataSortBuilder<TSort> DataSortBuilder => MessageDataSortBuilder;

    public TSort StateId(bool ascending)
    {
        return MessageDataSortBuilder.StateId(!ascending);
    }

    public TSort StateVersion(bool ascending)
    {
        return MessageDataSortBuilder.StateVersion(!ascending);
    }
}
