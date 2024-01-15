using EntityDb.Abstractions.States;
using MongoDB.Bson.Serialization;

namespace EntityDb.MongoDb.Documents.Serializers;

internal sealed class VersionSerializer : IBsonSerializer<StateVersion>
{
    public Type ValueType => typeof(StateVersion);

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public StateVersion Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var longValue = context.Reader.ReadInt64();

        var ulongValue = Convert.ToUInt64(longValue);

        return new StateVersion(ulongValue);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is not StateVersion version)
        {
            throw new NotSupportedException();
        }

        Serialize(context, args, version);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, StateVersion stateVersion)
    {
        var longValue = Convert.ToInt64(stateVersion.Value);

        context.Writer.WriteInt64(longValue);
    }
}
