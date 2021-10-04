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
            RenderedFieldDefinition? renderedParentField =
                _parentField.Render(parentDocumentSerializer, bsonSerializerRegistry);

            IBsonSerializer<TEmbeddedDocument>? childDocumentSerializer =
                bsonSerializerRegistry.GetSerializer<TEmbeddedDocument>();

            BsonDocument? renderedChildFilter = _childFilter.Render(childDocumentSerializer, bsonSerializerRegistry);

            BsonDocument? document = new BsonDocument();

            using BsonDocumentWriter? bsonWriter = new BsonDocumentWriter(document);

            EmbeddedFilterRewriter? embeddedFilterRewriter =
                new EmbeddedFilterRewriter(bsonWriter, renderedParentField.FieldName, _hoistedFieldNames);

            embeddedFilterRewriter.Rewrite(renderedChildFilter);

            return document;
        }
    }
}
