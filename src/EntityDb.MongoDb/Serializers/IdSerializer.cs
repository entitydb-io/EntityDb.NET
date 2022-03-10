using EntityDb.Abstractions.ValueObjects;
using MongoDB.Bson.Serialization;
using System;

namespace EntityDb.MongoDb.Serializers;

internal class IdSerializer : IBsonSerializer<Id>
{
    public Type ValueType => typeof(Id);
    
    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public Id Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var stringValue = context.Reader.ReadString();

        var value = Guid.Parse(stringValue);

        return new Id(value);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is not Id id)
        {
            throw new NotSupportedException();
        }

        Serialize(context, args, id);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Id id)
    {
        context.Writer.WriteString(id.Value.ToString());
    }
}
