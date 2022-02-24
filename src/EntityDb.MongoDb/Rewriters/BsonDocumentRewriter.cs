using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.MongoDb.Rewriters;

internal class BsonDocumentRewriter
{
    protected readonly BsonWriter _bsonWriter;

    public BsonDocumentRewriter(BsonWriter bsonWriter)
    {
        _bsonWriter = bsonWriter;
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
        _bsonWriter.WriteStartArray();

        foreach (var bsonValue in bsonValues)
        {
            Rewrite(bsonValue);
        }

        _bsonWriter.WriteEndArray();
    }

    protected virtual void RewriteDocument(BsonElement[] bsonElements)
    {
        _bsonWriter.WriteStartDocument();

        foreach (var bsonElement in bsonElements)
        {
            _bsonWriter.WriteName(bsonElement.Name);

            Rewrite(bsonElement.Value);
        }

        _bsonWriter.WriteEndDocument();
    }

    private void RewriteScalar(BsonValue bsonValue)
    {
        switch (bsonValue.BsonType)
        {
            case BsonType.Double:
                _bsonWriter.WriteDouble(bsonValue.AsDouble);
                break;

            case BsonType.String:
                _bsonWriter.WriteString(bsonValue.AsString);
                break;

            case BsonType.Boolean:
                _bsonWriter.WriteBoolean(bsonValue.AsBoolean);
                break;

            case BsonType.DateTime:
                _bsonWriter.WriteDateTime(bsonValue.AsBsonDateTime.MillisecondsSinceEpoch);
                break;

            case BsonType.Null:
                _bsonWriter.WriteNull();
                break;

            case BsonType.RegularExpression:
                _bsonWriter.WriteRegularExpression(bsonValue.AsBsonRegularExpression);
                break;

            case BsonType.Int32:
                _bsonWriter.WriteInt32(bsonValue.AsInt32);
                break;

            case BsonType.Timestamp:
                _bsonWriter.WriteTimestamp(bsonValue.AsBsonTimestamp.Value);
                break;

            case BsonType.Int64:
                _bsonWriter.WriteInt64(bsonValue.AsInt64);
                break;

            case BsonType.Decimal128:
                _bsonWriter.WriteDecimal128(bsonValue.AsDecimal128);
                break;

            default:
                throw new NotSupportedException();
        }
    }
}
