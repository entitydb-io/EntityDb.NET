using EntityDb.Abstractions.Queries.SortBuilders;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

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

    [Obsolete("This method will be removed in the future, and may not be supported for all implementations.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    public TSort AgentSignatureProperty<TAgentSignature>(bool ascending,
        Expression<Func<TAgentSignature, object>> agentSignatureExpression)
    {
        return AgentSignatureSortBuilder.AgentSignatureProperty(!ascending, agentSignatureExpression);
    }
}
