using EntityDb.Abstractions.States;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityDb.Json.Converters;

internal sealed class VersionConverter : JsonConverter<StateVersion>
{
    public override StateVersion Read
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

        return new StateVersion(ulongValue);
    }

    public override void Write
    (
        Utf8JsonWriter writer,
        StateVersion stateVersion,
        JsonSerializerOptions options
    )
    {
        var ulongValue = stateVersion.Value;

        writer.WriteNumberValue(ulongValue);
    }
}
