using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace EntityDb.MongoDb.Rewriters
{
    internal sealed class EmbeddedSortRewriter : HoistedRewriterBase
    {

        public EmbeddedSortRewriter(BsonWriter bsonWriter, string parentFieldName, string[] hoistedFieldNames) :
            base(bsonWriter, parentFieldName, hoistedFieldNames)
        {
        }

        protected override void RewriteDocument(BsonElement[] bsonElements)
        {
            RewriteHoisted(bsonElements);
        }
    }
}
