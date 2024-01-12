using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson.Serialization;

namespace EntityDb.MongoDb.Documents.Serializers;

internal sealed class TimeStampSerializer : IBsonSerializer<TimeStamp>
{
    public Type ValueType => typeof(TimeStamp);

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public TimeStamp Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var millisecondsSinceUnixEpoch = context.Reader.ReadDateTime();

        var value = DateTime.UnixEpoch + TimeSpan.FromMilliseconds(millisecondsSinceUnixEpoch);

        return new TimeStamp(value);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is not TimeStamp timeStamp)
        {
            throw new NotSupportedException();
        }

        Serialize(context, args, timeStamp);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeStamp timeStamp)
    {
        var millisecondsSinceUnixEpoch =
            Convert.ToInt64(Math.Floor((timeStamp.Value - DateTime.UnixEpoch).TotalMilliseconds));

        context.Writer.WriteDateTime(millisecondsSinceUnixEpoch);
    }
}
