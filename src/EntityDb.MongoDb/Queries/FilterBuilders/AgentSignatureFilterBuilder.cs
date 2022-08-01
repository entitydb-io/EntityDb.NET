using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.FilterBuilders;

internal sealed class AgentSignatureFilterBuilder : FilterBuilderBase,
    IAgentSignatureFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> EntityIdsIn(params Id[] entityIds)
    {
        return AnyIn(nameof(AgentSignatureDocument.EntityIds), entityIds);
    }

    public FilterDefinition<BsonDocument> AgentSignatureTypeIn(params Type[] agentSignatureTypes)
    {
        return DataTypeIn(agentSignatureTypes);
    }

    [Obsolete("This method will be removed in the future, and may not be supported for all implementations.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    public FilterDefinition<BsonDocument> AgentSignatureMatches<TAgentSignature>(
        Expression<Func<TAgentSignature, bool>> agentSignatureExpression)
    {
        return DataValueMatches(agentSignatureExpression);
    }
}
