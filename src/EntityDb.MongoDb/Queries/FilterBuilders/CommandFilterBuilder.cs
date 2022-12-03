using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.FilterBuilders;

internal sealed class CommandFilterBuilder : FilterBuilderBase,
    ICommandFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> EntityIdIn(params Id[] entityIds)
    {
        return In(nameof(CommandDocument.EntityId), entityIds);
    }

    public FilterDefinition<BsonDocument> EntityVersionNumberGte(VersionNumber entityVersionNumber)
    {
        return Gte(nameof(CommandDocument.EntityVersionNumber), entityVersionNumber);
    }

    public FilterDefinition<BsonDocument> EntityVersionNumberLte(VersionNumber entityVersionNumber)
    {
        return Lte(nameof(CommandDocument.EntityVersionNumber), entityVersionNumber);
    }

    public FilterDefinition<BsonDocument> CommandTypeIn(params Type[] commandTypes)
    {
        return DataTypeIn(commandTypes);
    }
}
