using EntityDb.Common.Envelopes;
using EntityDb.Common.TypeResolvers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EntityDb.Json.Envelopes;

internal sealed class JsonBytesEnvelopeService : JsonEnvelopeService<byte[]>
{
    public JsonBytesEnvelopeService(ILogger<JsonBytesEnvelopeService> logger, ITypeResolver typeResolver) : base(logger,
        typeResolver)
    {
    }

    protected override Envelope<JsonElement> DeserializeEnvelope(byte[] serializedData)
    {
        return (Envelope<JsonElement>)JsonSerializer.Deserialize(serializedData, typeof(Envelope<JsonElement>),
            JsonSerializerOptions)!;
    }

    protected override byte[] SerializeEnvelope(Envelope<JsonElement> envelope)
    {
        return JsonSerializer.SerializeToUtf8Bytes(envelope, typeof(Envelope<JsonElement>), JsonSerializerOptions);
    }
}
