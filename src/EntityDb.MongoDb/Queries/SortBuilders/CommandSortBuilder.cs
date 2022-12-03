using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.SortBuilders;

internal sealed class CommandSortBuilder : SortBuilderBase, ICommandSortBuilder<SortDefinition<BsonDocument>>
{
    public SortDefinition<BsonDocument> EntityId(bool ascending)
    {
        return Sort(ascending, nameof(CommandDocument.EntityId));
    }

    public SortDefinition<BsonDocument> EntityVersionNumber(bool ascending)
    {
        return Sort(ascending, nameof(CommandDocument.EntityVersionNumber));
    }

    public SortDefinition<BsonDocument> CommandType(bool ascending)
    {
        return SortDataType(ascending);
    }
}
