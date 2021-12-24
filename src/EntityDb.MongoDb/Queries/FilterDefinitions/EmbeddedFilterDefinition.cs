using EntityDb.MongoDb.Rewriters;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.FilterDefinitions
{
    internal sealed class
        EmbeddedFilterDefinition<TParentDocument, TEmbeddedDocument> : FilterDefinition<TParentDocument>
    {
        private readonly FilterDefinition<TEmbeddedDocument> _childFilter;
        private readonly string[] _hoistedFieldNames;
        private readonly FieldDefinition<TParentDocument> _parentField;

        public EmbeddedFilterDefinition(FieldDefinition<TParentDocument> parentField,
            FilterDefinition<TEmbeddedDocument> childFilter, string[] hoistedFieldNames)
        {
            _parentField = parentField;
            _childFilter = childFilter;
            _hoistedFieldNames = hoistedFieldNames;
        }

        public override BsonDocument Render(IBsonSerializer<TParentDocument> parentDocumentSerializer,
            IBsonSerializerRegistry bsonSerializerRegistry)
        {
            var renderedParentField = _parentField.Render(parentDocumentSerializer, bsonSerializerRegistry);

            var childDocumentSerializer = bsonSerializerRegistry.GetSerializer<TEmbeddedDocument>();

            var renderedChildFilter = _childFilter.Render(childDocumentSerializer, bsonSerializerRegistry);

            var document = new BsonDocument();

            using var bsonWriter = new BsonDocumentWriter(document);

            var embeddedFilterRewriter =
                new HoistedRewriter(bsonWriter, renderedParentField.FieldName, _hoistedFieldNames);

            embeddedFilterRewriter.Rewrite(renderedChildFilter);

            return document;
        }
    }
}
