using EntityDb.Abstractions.ValueObjects;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityDb.SqlDb.Converters;

internal class VersionNumberConverter : JsonConverter<VersionNumber>
{
    public override VersionNumber Read
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new NotSupportedException();
        }

        var ulongValue = reader.GetUInt64();

        return new VersionNumber(ulongValue);
    }

    public override void Write
    (
        Utf8JsonWriter writer,
        VersionNumber versionNumber,
        JsonSerializerOptions options
    )
    {
        var ulongValue = versionNumber.Value;

        writer.WriteNumberValue(ulongValue);
    }
}
