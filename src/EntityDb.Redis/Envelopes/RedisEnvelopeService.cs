using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using EntityDb.Common.TypeResolvers;
using EntityDb.Redis.Converters;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityDb.Redis.Envelopes;

internal sealed class RedisEnvelopeService : IEnvelopeService<byte[]>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ILogger<RedisEnvelopeService> _logger;
    private readonly ITypeResolver _typeResolver;

    static RedisEnvelopeService()
    {
        JsonSerializerOptions.Converters.Add(new EnvelopeHeadersConverter());
        JsonSerializerOptions.Converters.Add(new IdConverter());
        JsonSerializerOptions.Converters.Add(new VersionNumberConverter());
    }

    public RedisEnvelopeService
    (
        ILogger<RedisEnvelopeService> logger,
        ITypeResolver typeResolver
    )
    {
        _logger = logger;
        _typeResolver = typeResolver;
    }

    public byte[] Serialize<TData>(TData data)
    {
        try
        {
            var dataType = data!.GetType();

            var json = JsonSerializer.Serialize(data, dataType, JsonSerializerOptions);

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json, JsonSerializerOptions);

            var headers = EnvelopeHelper.GetEnvelopeHeaders(dataType);

            var envelope = new Envelope<JsonElement>(headers, jsonElement);

            return JsonSerializer.SerializeToUtf8Bytes(envelope, typeof(Envelope<JsonElement>), JsonSerializerOptions);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to serialize");

            throw new SerializeException();
        }
    }

    public TData Deserialize<TData>(byte[] serializedData)
    {
        try
        {
            var envelope = (Envelope<JsonElement>)JsonSerializer.Deserialize(serializedData, typeof(Envelope<JsonElement>),
                JsonSerializerOptions)!;

            return (TData)JsonSerializer.Deserialize(envelope.Value.GetRawText(),
                _typeResolver.ResolveType(envelope.Headers), JsonSerializerOptions)!;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to deserialize");

            throw new DeserializeException();
        }
    }
}
