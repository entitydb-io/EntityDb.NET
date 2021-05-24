using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Linq;

namespace EntityDb.MongoDb.Rewriters
{
    internal sealed class EmbeddedFilterRewriter : BsonDocumentRewriter
    {
        private static readonly string[] _booleanOperators = new[] { "$not", "$and", "$or" };

        private readonly string _parentFieldName;
        private readonly string[] _hoistedFieldNames;

        private bool FoundTopDocument = false;

        public EmbeddedFilterRewriter(BsonWriter bsonWriter, string parentFieldName, string[] hoistedFieldNames) : base(bsonWriter)
        {
            _parentFieldName = parentFieldName;
            _hoistedFieldNames = hoistedFieldNames;
        }

        protected override void RewriteDocument(BsonElement[] bsonElements)
        {
            if (FoundTopDocument || IsBooleanOperatorDocument(bsonElements))
            {
                base.RewriteDocument(bsonElements);
            }
            else
            {
                FoundTopDocument = true;

                RewriteTopDocument(bsonElements);
            }
        }

        private static bool IsBooleanOperatorDocument(BsonElement[] bsonElements)
        {
            return bsonElements.Length == 1 && _booleanOperators.Contains(bsonElements[0].Name);
        }

        private void RewriteTopDocument(BsonElement[] bsonElements)
        {
            _bsonWriter.WriteStartDocument();

            foreach (var bsonElement in bsonElements)
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
