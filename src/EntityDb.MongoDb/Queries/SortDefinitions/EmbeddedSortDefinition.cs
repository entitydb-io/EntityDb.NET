using EntityDb.MongoDb.Rewriters;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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
            IBsonSerializerRegistry bsonSerializerRegistry, LinqProvider linqProvider)
        {
            var renderedParentField = _parentField.Render(parentDocumentSerializer, bsonSerializerRegistry, linqProvider);

            var childDocumentSerializer = bsonSerializerRegistry.GetSerializer<TEmbeddedDocument>();

            var renderedChildSort = _childSort.Render(childDocumentSerializer, bsonSerializerRegistry, linqProvider);

            var document = new BsonDocument();

            using var bsonWriter = new BsonDocumentWriter(document);

            var embeddedSortRewriter =
                new HoistedRewriter(bsonWriter, renderedParentField.FieldName, _hoistedFieldNames);

            embeddedSortRewriter.Rewrite(renderedChildSort);

            return document;
        }
    }
}
