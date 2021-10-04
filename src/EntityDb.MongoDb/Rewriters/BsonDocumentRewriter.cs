using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Linq;

namespace EntityDb.MongoDb.Rewriters
{
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

        protected virtual void RewriteArray(BsonValue[] bsonValues)
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

            foreach (BsonElement bsonElement in bsonElements)
            {
                _bsonWriter.WriteName(bsonElement.Name);

                Rewrite(bsonElement.Value);
            }

            _bsonWriter.WriteEndDocument();
        }

        protected virtual void RewriteScalar(BsonValue bsonValue)
        {
            switch (bsonValue.BsonType)
            {
                case BsonType.Double:
                    _bsonWriter.WriteDouble(bsonValue.AsDouble);
                    break;

                case BsonType.String:
                    _bsonWriter.WriteString(bsonValue.AsString);
                    break;

                case BsonType.Binary:
                    _bsonWriter.WriteBinaryData(bsonValue.AsBsonBinaryData);
                    break;

                case BsonType.Undefined:
                    _bsonWriter.WriteUndefined();
                    break;

                case BsonType.ObjectId:
                    _bsonWriter.WriteObjectId(bsonValue.AsObjectId);
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

                case BsonType.Symbol:
                    _bsonWriter.WriteSymbol(bsonValue.AsBsonSymbol.Name);
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

                case BsonType.MaxKey:
                    _bsonWriter.WriteMaxKey();
                    break;

                case BsonType.MinKey:
                    _bsonWriter.WriteMinKey();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
