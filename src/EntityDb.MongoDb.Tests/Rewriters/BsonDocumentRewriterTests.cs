using EntityDb.MongoDb.Rewriters;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Shouldly;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace EntityDb.MongoDb.Tests.Rewriters
{
    public class BsonDocumentRewriterTests
    {
        private BsonDocument CreateSubDocument()
        {
            return new BsonDocument
            {
                ["Double"] = BsonDouble.Create(0.0),
                ["String"] = BsonString.Create(""),
                ["Binary"] = BsonBinaryData.Create(new byte[] { 0, 1, 2 }),
                ["Undefined"] = BsonUndefined.Value,
                ["ObjectId"] = BsonObjectId.Empty,
                ["Boolean"] = BsonBoolean.True,
                ["DateTime"] = BsonDateTime.Create(DateTime.UtcNow),
                ["Null"] = BsonNull.Value,
                ["RegularExpression"] = BsonRegularExpression.Create(new Regex("$abc^")),
                ["Symbol"] = BsonSymbol.Create(""),
                ["Int32"] = BsonInt32.Create(0),
                ["Timestamp"] =
                    BsonTimestamp.Create(Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds)),
                ["Int64"] = BsonInt64.Create((long)0),
                ["Decimal128"] = BsonDecimal128.Create((decimal)0.0),
                ["MaxKey"] = BsonMaxKey.Value,
                ["MinKey"] = BsonMinKey.Value
            };
        }

        [Fact]
        public void CanRewriteFullBsonDocument()
        {
            // ARRANGE

            BsonDocument? originalBsonDocument = new BsonDocument
            {
                ["Document"] = CreateSubDocument(),
                ["Array"] = new BsonArray { CreateSubDocument(), CreateSubDocument() }
            };

            byte[]? expectedBson = originalBsonDocument.ToBson();

            BsonDocument? copyBsonDocument = new BsonDocument();

            using BsonDocumentWriter? bsonWriter = new BsonDocumentWriter(copyBsonDocument);

            BsonDocumentRewriter? bsonDocumentRewriter = new BsonDocumentRewriter(bsonWriter);

            // ACT

            bsonDocumentRewriter.Rewrite(originalBsonDocument);

            byte[]? actualBson = copyBsonDocument.ToBson();

            // ASSERT

            actualBson.SequenceEqual(expectedBson).ShouldBeTrue();
        }

        [Fact]
        public void CannotWriteBsonDocumentWithJavaScript()
        {
            // ARRANGE

            BsonDocument? originalBsonDocument = new BsonDocument
            {
                ["JavaScript"] = BsonJavaScript.Create("function() { return true; }"),
                ["JavaScriptWithScope"] = BsonJavaScriptWithScope.Create("function(a) { return true; }")
            };

            BsonDocument? copyBsonDocument = new BsonDocument();

            using BsonDocumentWriter? bsonWriter = new BsonDocumentWriter(copyBsonDocument);

            BsonDocumentRewriter? bsonDocumentRewriter = new BsonDocumentRewriter(bsonWriter);

            // ASSERT

            Should.Throw<NotSupportedException>(() =>
            {
                bsonDocumentRewriter.Rewrite(originalBsonDocument);
            });
        }
    }
}
