using EntityDb.Abstractions.ValueObjects;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityDb.SqlDb.Converters;

internal class IdConverter : JsonConverter<Id>
{
    public override Id Read
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new NotSupportedException();
        }

        var stringValue = reader.GetString();

        var guidValue = Guid.Parse(stringValue!);

        return new Id(guidValue);
    }

    public override void Write
    (
        Utf8JsonWriter writer,
        Id id,
        JsonSerializerOptions options
    )
    {
        var stringValue = id.Value.ToString();

        writer.WriteStringValue(stringValue);
    }
}
