using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson.Serialization;

namespace EntityDb.MongoDb.Serializers;

internal class VersionNumberSerializer : IBsonSerializer<VersionNumber>
{
    public Type ValueType => typeof(VersionNumber);

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public VersionNumber Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var longValue = context.Reader.ReadInt64();

        var ulongValue = Convert.ToUInt64(longValue);

        return new VersionNumber(ulongValue);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is not VersionNumber versionNumber)
        {
            throw new NotSupportedException();
        }

        Serialize(context, args, versionNumber);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, VersionNumber versionNumber)
    {
        var longValue = Convert.ToInt64(versionNumber.Value);

        context.Writer.WriteInt64(longValue);
    }
}
