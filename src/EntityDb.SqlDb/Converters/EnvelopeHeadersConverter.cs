using EntityDb.Common.Envelopes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityDb.SqlDb.Converters;

internal class EnvelopeHeadersConverter : JsonConverter<EnvelopeHeaders>
{
    public override EnvelopeHeaders Read
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = new Dictionary<string, string>();

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var propertyName = reader.GetString()!;

            reader.Read();

            var propertyValue = reader.GetString()!;

            value.Add(propertyName, propertyValue);
        }

        return new EnvelopeHeaders(value);
    }

    public override void Write
    (
        Utf8JsonWriter writer,
        EnvelopeHeaders envelopeHeaders,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        foreach (var (propertyName, propertyValue) in envelopeHeaders.Value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteStringValue(propertyValue);
        }

        writer.WriteEndObject();
    }
}
