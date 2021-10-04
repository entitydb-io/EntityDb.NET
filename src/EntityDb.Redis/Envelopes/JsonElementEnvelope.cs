using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Envelopes;
using EntityDb.Redis.Exceptions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EntityDb.Redis.Envelopes
{
    internal sealed record JsonElementEnvelope
    {
        public Dictionary<string, string> Headers { get; init; } = default!;
        public JsonElement Value { get; init; }

        private static JsonElement GetJsonElement(dynamic? @object)
        {
            var json = JsonSerializer.Serialize(@object);

            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        public TObject Reconstruct<TObject>(ILogger logger, IResolvingStrategyChain resolvingStrategyChain)
        {
            try
            {
                return (TObject)JsonSerializer.Deserialize(Value.GetRawText(),
                    resolvingStrategyChain.ResolveType(Headers))!;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to reconstruct.");

                throw new DeserializeException();
            }
        }

        public byte[] Serialize(ILogger logger)
        {
            try
            {
                return JsonSerializer.SerializeToUtf8Bytes(this, typeof(JsonElementEnvelope));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to serialize.");

                throw new SerializeException();
            }
        }

        public static JsonElementEnvelope Deserialize(byte[] utf8JsonBytes, ILogger logger)
        {
            try
            {
                return (JsonElementEnvelope)JsonSerializer.Deserialize(utf8JsonBytes, typeof(JsonElementEnvelope))!;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to deserialize.");

                throw new DeserializeException();
            }
        }

        public static JsonElementEnvelope Deconstruct(dynamic? @object, ILogger logger)
        {
            try
            {
                var jsonElement = GetJsonElement(@object);

                var headers = EnvelopeHelper.GetTypeHeaders((@object as object)!.GetType());

                return new JsonElementEnvelope { Headers = headers, Value = jsonElement };
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to deconstruct.");

                throw new SerializeException();
            }
        }
    }
}
