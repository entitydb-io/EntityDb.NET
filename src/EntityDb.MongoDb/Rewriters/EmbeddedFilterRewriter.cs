using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Linq;

namespace EntityDb.MongoDb.Rewriters
{
    internal sealed class EmbeddedFilterRewriter : HoistedRewriterBase
    {
        private static readonly string[] _booleanOperators = { "$not", "$and", "$or" };

        private bool FoundTopDocument;

        public EmbeddedFilterRewriter(BsonWriter bsonWriter, string parentFieldName, string[] hoistedFieldNames) :
            base(bsonWriter, parentFieldName, hoistedFieldNames)
        {
        }

        protected override void RewriteDocument(BsonElement[] bsonElements)
        {
            if (IsBooleanOperatorDocument(bsonElements))
            {
                var rewriter = new EmbeddedFilterRewriter(_bsonWriter, _parentFieldName, _hoistedFieldNames);

                _bsonWriter.WriteStartDocument(bsonElements[0].Name);

                rewriter.Rewrite(bsonElements[0].Value);

                _bsonWriter.WriteEndDocument();
            }
            else if (FoundTopDocument)
            {
                base.RewriteDocument(bsonElements);
            }
            else
            {
                FoundTopDocument = true;

                RewriteHoisted(bsonElements);
            }
        }

        private static bool IsBooleanOperatorDocument(BsonElement[] bsonElements)
        {
            return bsonElements.Length == 1 && _booleanOperators.Contains(bsonElements[0].Name);
        }
    }
}
