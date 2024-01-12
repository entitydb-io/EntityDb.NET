using MongoDB.Bson.Serialization;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Documents.Serializers;

internal sealed class VersionSerializer : IBsonSerializer<Version>
{
    public Type ValueType => typeof(Version);

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public Version Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var longValue = context.Reader.ReadInt64();

        var ulongValue = Convert.ToUInt64(longValue);

        return new Version(ulongValue);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is not Version version)
        {
            throw new NotSupportedException();
        }

        Serialize(context, args, version);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Version version)
    {
        var longValue = Convert.ToInt64(version.Value);

        context.Writer.WriteInt64(longValue);
    }
}
