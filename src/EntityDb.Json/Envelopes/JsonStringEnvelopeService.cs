using EntityDb.Common.Envelopes;
using EntityDb.Common.TypeResolvers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EntityDb.Json.Envelopes;

internal sealed class JsonStringEnvelopeService : JsonEnvelopeService<string>
{
    public JsonStringEnvelopeService(ILogger<JsonStringEnvelopeService> logger, ITypeResolver typeResolver) : base(logger, typeResolver)
    {
    }

    protected override Envelope<JsonElement> DeserializeEnvelope(string serializedData)
    {
        return (Envelope<JsonElement>)JsonSerializer.Deserialize(serializedData, typeof(Envelope<JsonElement>),
                JsonSerializerOptions)!;
    }

    protected override string SerializeEnvelope(Envelope<JsonElement> envelope)
    {
        return JsonSerializer.Serialize(envelope, typeof(Envelope<JsonElement>), JsonSerializerOptions);
    }
}
