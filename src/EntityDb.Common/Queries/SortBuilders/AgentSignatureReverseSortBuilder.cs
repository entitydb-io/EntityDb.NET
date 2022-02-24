using EntityDb.Abstractions.Queries.SortBuilders;
using System;
using System.Linq.Expressions;

namespace EntityDb.Common.Queries.SortBuilders;

internal sealed record AgentSignatureReverseSortBuilder<TSort>
    (IAgentSignatureSortBuilder<TSort> AgentSignatureSortBuilder) : ReverseSortBuilderBase<TSort>(AgentSignatureSortBuilder),
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

    public TSort AgentSignatureProperty<TAgentSignature>(bool ascending, Expression<Func<TAgentSignature, object>> agentSignatureExpression)
    {
        return AgentSignatureSortBuilder.AgentSignatureProperty(!ascending, agentSignatureExpression);
    }
}
