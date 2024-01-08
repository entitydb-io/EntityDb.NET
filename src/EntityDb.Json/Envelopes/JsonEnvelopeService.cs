using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using EntityDb.Common.TypeResolvers;
using EntityDb.Json.Converters;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityDb.Json.Envelopes;

internal abstract class JsonEnvelopeService
{
    protected static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}

internal abstract class JsonEnvelopeService<TSerializedData> : JsonEnvelopeService, IEnvelopeService<TSerializedData>
{
    private readonly ILogger<JsonEnvelopeService<TSerializedData>> _logger;
    private readonly ITypeResolver _typeResolver;

    static JsonEnvelopeService()
    {
        JsonSerializerOptions.Converters.Add(new EnvelopeHeadersConverter());
        JsonSerializerOptions.Converters.Add(new IdConverter());
        JsonSerializerOptions.Converters.Add(new VersionConverter());
    }

    protected JsonEnvelopeService
    (
        ILogger<JsonEnvelopeService<TSerializedData>> logger,
        ITypeResolver typeResolver
    )
    {
        _logger = logger;
        _typeResolver = typeResolver;
    }

    public TSerializedData Serialize<TData>(TData data)
    {
        try
        {
            var dataType = data!.GetType();

            var json = JsonSerializer.Serialize(data, dataType, JsonSerializerOptions);

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json, JsonSerializerOptions);

            var headers = EnvelopeHelper.GetEnvelopeHeaders(dataType);

            var envelope = new Envelope<JsonElement>(headers, jsonElement);

            return SerializeEnvelope(envelope);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to serialize");

            throw new SerializeException();
        }
    }

    public TData Deserialize<TData>(TSerializedData serializedData)
    {
        try
        {
            var envelope = DeserializeEnvelope(serializedData);

            return (TData)JsonSerializer.Deserialize(envelope.Value.GetRawText(),
                _typeResolver.ResolveType(envelope.Headers), JsonSerializerOptions)!;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to deserialize");

            throw new DeserializeException();
        }
    }

    protected abstract TSerializedData SerializeEnvelope(Envelope<JsonElement> envelope);

    protected abstract Envelope<JsonElement> DeserializeEnvelope(TSerializedData serializedData);
}
