using System.Text.Json;
using System.Text.Json.Serialization;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Json.Converters;

internal sealed class VersionConverter : JsonConverter<Version>
{
    public override Version Read
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

        return new Version(ulongValue);
    }

    public override void Write
    (
        Utf8JsonWriter writer,
        Version version,
        JsonSerializerOptions options
    )
    {
        var ulongValue = version.Value;

        writer.WriteNumberValue(ulongValue);
    }
}
