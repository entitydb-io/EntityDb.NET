using EntityDb.MongoDb.Rewriters;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries.SortDefinitions
{
    internal class EmbeddedSortDefinition<TParentDocument, TEmbeddedDocument> : SortDefinition<TParentDocument>
    {
        private readonly SortDefinition<TEmbeddedDocument> _childSort;
        private readonly string[] _hoistedFieldNames;
        private readonly FieldDefinition<TParentDocument> _parentField;

        public EmbeddedSortDefinition(FieldDefinition<TParentDocument> parentField,
            SortDefinition<TEmbeddedDocument> childSort, string[] hoistedFieldNames)
        {
            _parentField = parentField;
            _childSort = childSort;
            _hoistedFieldNames = hoistedFieldNames;
        }

        public override BsonDocument Render(IBsonSerializer<TParentDocument> parentDocumentSerializer,
            IBsonSerializerRegistry bsonSerializerRegistry)
        {
            RenderedFieldDefinition? renderedParentField =
                _parentField.Render(parentDocumentSerializer, bsonSerializerRegistry);

            IBsonSerializer<TEmbeddedDocument>? childDocumentSerializer =
                bsonSerializerRegistry.GetSerializer<TEmbeddedDocument>();

            BsonDocument? renderedChildSort = _childSort.Render(childDocumentSerializer, bsonSerializerRegistry);

            BsonDocument? document = new BsonDocument();

            using BsonDocumentWriter? bsonWriter = new BsonDocumentWriter(document);

            EmbeddedSortRewriter? embeddedSortRewriter =
                new EmbeddedSortRewriter(bsonWriter, renderedParentField.FieldName, _hoistedFieldNames);

            embeddedSortRewriter.Rewrite(renderedChildSort);

            return document;
        }
    }
}
