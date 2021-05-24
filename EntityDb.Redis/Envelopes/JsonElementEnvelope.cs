using EntityDb.Common.Extensions;
using EntityDb.Common.Strategies.Resolving;
using EntityDb.Redis.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace EntityDb.Redis.Envelopes
{
    internal sealed record JsonElementEnvelope
    (
        string? AssemblyFullName,
        string? TypeFullName,
        string? TypeName,
        JsonElement Value
    )
    {
        private static JsonElement GetJsonElement(dynamic? @object)
        {
            var json = JsonSerializer.Serialize(@object);

            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        public TObject Reconstruct<TObject>(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(JsonElementEnvelope));

            try
            {
                return (TObject)JsonSerializer.Deserialize(Value.GetRawText(), serviceProvider.ResolveType(AssemblyFullName, TypeFullName, TypeName))!;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.GetHashCode(), exception, "Unable to reconstruct.");

                throw new DeserializeException();
            }
        }

        public byte[] Serialize(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(JsonElementEnvelope));

            try
            {
                return JsonSerializer.SerializeToUtf8Bytes(this, typeof(JsonElementEnvelope));
            }
            catch (Exception exception)
            {
                logger.LogError(exception.GetHashCode(), exception, "Unable to serialize.");

                throw new SerializeException();
            }
        }

        public static JsonElementEnvelope Deserialize(byte[] utf8JsonBytes, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(JsonElementEnvelope));

            try
            {
                return (JsonElementEnvelope)JsonSerializer.Deserialize(utf8JsonBytes, typeof(JsonElementEnvelope))!;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.GetHashCode(), exception, "Unable to deserialize.");

                throw new DeserializeException();
            }
        }

        public static JsonElementEnvelope Deconstruct(dynamic? @object, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(JsonElementEnvelope));

            try
            {
                var jsonElement = GetJsonElement(@object);

                var (assemblyFullName, typeFullName, typeName) = (@object as object)!.GetType().GetTypeInfo();

                return new JsonElementEnvelope
                (
                    assemblyFullName,
                    typeFullName,
                    typeName,
                    jsonElement
                );
            }
            catch (Exception exception)
            {
                logger.LogError(exception.GetHashCode(), exception, "Unable to deconstruct.");

                throw new SerializeException();
            }
        }
    }
}
