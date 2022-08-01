using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace EntityDb.MongoDb.Rewriters;

internal class BsonDocumentRewriter
{
    protected readonly BsonWriter BsonWriter;

    public BsonDocumentRewriter(BsonWriter bsonWriter)
    {
        BsonWriter = bsonWriter;
    }

    public void Rewrite(BsonValue bsonValue)
    {
        switch (bsonValue.BsonType)
        {
            case BsonType.Array:
                RewriteArray(bsonValue.AsBsonArray.ToArray());
                break;

            case BsonType.Document:
                RewriteDocument(bsonValue.AsBsonDocument.ToArray());
                break;

            default:
                RewriteScalar(bsonValue);
                break;
        }
    }

    private void RewriteArray(IEnumerable<BsonValue> bsonValues)
    {
        BsonWriter.WriteStartArray();

        foreach (var bsonValue in bsonValues)
        {
            Rewrite(bsonValue);
        }

        BsonWriter.WriteEndArray();
    }

    protected virtual void RewriteDocument(BsonElement[] bsonElements)
    {
        BsonWriter.WriteStartDocument();

        foreach (var bsonElement in bsonElements)
        {
            BsonWriter.WriteName(bsonElement.Name);

            Rewrite(bsonElement.Value);
        }

        BsonWriter.WriteEndDocument();
    }

    private void RewriteScalar(BsonValue bsonValue)
    {
        switch (bsonValue.BsonType)
        {
            case BsonType.Double:
                BsonWriter.WriteDouble(bsonValue.AsDouble);
                break;

            case BsonType.String:
                BsonWriter.WriteString(bsonValue.AsString);
                break;

            case BsonType.Boolean:
                BsonWriter.WriteBoolean(bsonValue.AsBoolean);
                break;

            case BsonType.DateTime:
                BsonWriter.WriteDateTime(bsonValue.AsBsonDateTime.MillisecondsSinceEpoch);
                break;

            case BsonType.Null:
                BsonWriter.WriteNull();
                break;

            case BsonType.RegularExpression:
                BsonWriter.WriteRegularExpression(bsonValue.AsBsonRegularExpression);
                break;

            case BsonType.Int32:
                BsonWriter.WriteInt32(bsonValue.AsInt32);
                break;

            case BsonType.Timestamp:
                BsonWriter.WriteTimestamp(bsonValue.AsBsonTimestamp.Value);
                break;

            case BsonType.Int64:
                BsonWriter.WriteInt64(bsonValue.AsInt64);
                break;

            case BsonType.Decimal128:
                BsonWriter.WriteDecimal128(bsonValue.AsDecimal128);
                break;

            default:
                throw new NotSupportedException();
        }
    }
}
