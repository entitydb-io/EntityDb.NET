using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace EntityDb.MongoDb.Queries.FilterBuilders;

internal sealed class TagFilterBuilder : FilterBuilderBase, ITagFilterBuilder<FilterDefinition<BsonDocument>>
{
    public FilterDefinition<BsonDocument> EntityIdIn(params Id[] entityIds)
    {
        return In(nameof(TagDocument.EntityId), entityIds);
    }

    public FilterDefinition<BsonDocument> EntityVersionNumberGte(VersionNumber entityVersionNumber)
    {
        return Gte(nameof(TagDocument.EntityVersionNumber), entityVersionNumber);
    }

    public FilterDefinition<BsonDocument> EntityVersionNumberLte(VersionNumber entityVersionNumber)
    {
        return Lte(nameof(TagDocument.EntityVersionNumber), entityVersionNumber);
    }

    public FilterDefinition<BsonDocument> TagTypeIn(params Type[] tagTypes)
    {
        return DataTypeIn(tagTypes);
    }

    public FilterDefinition<BsonDocument> TagMatches<TTag>(Expression<Func<TTag, bool>> tagExpression)
    {
        return DataValueMatches(tagExpression);
    }

    protected override string[] GetHoistedFieldNames()
    {
        return TagDocument.HoistedFieldNames;
    }
}
