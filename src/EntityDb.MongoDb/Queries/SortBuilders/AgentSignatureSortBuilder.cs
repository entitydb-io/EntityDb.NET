using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.SortBuilders;

internal sealed class AgentSignatureSortBuilder : SortBuilderBase,
    IAgentSignatureSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> EntityIds(bool ascending)
    {
        return Sort(ascending, nameof(AgentSignatureDocument.EntityIds));
    }

    public SortDefinition<BsonDocument> AgentSignatureType(bool ascending)
    {
        return SortDataType(ascending);
    }

    public SortDefinition<BsonDocument> AgentSignatureProperty<TAgentSignature>(bool ascending,
        Expression<Func<TAgentSignature, object>> agentSignatureExpression)
    {
        return SortDataValue(ascending, agentSignatureExpression);
    }
}
