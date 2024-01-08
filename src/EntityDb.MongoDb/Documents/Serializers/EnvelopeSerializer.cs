using EntityDb.Common.Envelopes;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace EntityDb.MongoDb.Documents.Serializers;

internal class EnvelopeSerializer : IBsonSerializer<Envelope<BsonDocument>>
{
    private static readonly BsonDocumentSerializer BsonDocumentSerializer = new();

    public Type ValueType => typeof(Envelope<BsonDocument>);

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public Envelope<BsonDocument> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        context.Reader.ReadStartDocument();

        context.Reader.ReadName(Utf8NameDecoder.Instance);
        var headers = DeserializeHeaders(context);

        context.Reader.ReadName(Utf8NameDecoder.Instance);
        var value = BsonDocumentSerializer.Deserialize(context, args);

        context.Reader.ReadEndDocument();

        return new Envelope<BsonDocument>(headers, value);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is not Envelope<BsonDocument> envelope)
        {
            throw new NotSupportedException();
        }

        Serialize(context, args, envelope);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Envelope<BsonDocument> envelope)
    {
        context.Writer.WriteStartDocument();

        context.Writer.WriteName(nameof(envelope.Headers));
        SerializeHeaders(context, envelope.Headers);

        context.Writer.WriteName(nameof(envelope.Value));
        BsonDocumentSerializer.Serialize(context, args, envelope.Value);

        context.Writer.WriteEndDocument();
    }

    private static EnvelopeHeaders DeserializeHeaders(BsonDeserializationContext context)
    {
        var value = new Dictionary<string, string>();

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var headerName = context.Reader.ReadName(Utf8NameDecoder.Instance);
            var headerValue = context.Reader.ReadString();

            value.Add(headerName, headerValue);
        }

        context.Reader.ReadEndDocument();

        return new EnvelopeHeaders(value);
    }

    private static void SerializeHeaders(BsonSerializationContext context, EnvelopeHeaders envelopeHeaders)
    {
        context.Writer.WriteStartDocument();

        foreach (var (headerName, headerValue) in envelopeHeaders.Value)
        {
            context.Writer.WriteName(headerName);

            context.Writer.WriteString(headerValue);
        }

        context.Writer.WriteEndDocument();
    }
}
