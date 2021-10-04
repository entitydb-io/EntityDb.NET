using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Linq;

namespace EntityDb.MongoDb.Rewriters
{
    internal sealed class EmbeddedSortRewriter : BsonDocumentRewriter
    {
        private readonly string[] _hoistedFieldNames;
        private readonly string _parentFieldName;

        public EmbeddedSortRewriter(BsonWriter bsonWriter, string parentFieldName, string[] hoistedFieldNames) :
            base(bsonWriter)
        {
            _parentFieldName = parentFieldName;
            _hoistedFieldNames = hoistedFieldNames;
        }

        protected override void RewriteDocument(BsonElement[] bsonElements)
        {
            RewriteTopDocument(bsonElements);
        }

        private void RewriteTopDocument(BsonElement[] bsonElements)
        {
            _bsonWriter.WriteStartDocument();

            foreach (BsonElement bsonElement in bsonElements)
            {
                if (_hoistedFieldNames.Contains(bsonElement.Name))
                {
                    _bsonWriter.WriteName(bsonElement.Name);
                }
                else
                {
                    _bsonWriter.WriteName(_parentFieldName + "." + bsonElement.Name);
                }

                Rewrite(bsonElement.Value);
            }

            _bsonWriter.WriteEndDocument();
        }
    }
}
