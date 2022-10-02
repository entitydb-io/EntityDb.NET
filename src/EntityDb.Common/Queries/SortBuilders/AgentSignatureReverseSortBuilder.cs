using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Common.Queries.SortBuilders;

internal sealed record AgentSignatureReverseSortBuilder<TSort>
    (IAgentSignatureSortBuilder<TSort> AgentSignatureSortBuilder) : ReverseSortBuilderBase<TSort>(
            AgentSignatureSortBuilder),
        IAgentSignatureSortBuilder<TSort>
{
    public TSort EntityIds(bool ascending)
    {
        return AgentSignatureSortBuilder.EntityIds(!ascending);
    }

    public TSort AgentSignatureType(bool ascending)
    {
        return AgentSignatureSortBuilder.AgentSignatureType(!ascending);
    }
}
