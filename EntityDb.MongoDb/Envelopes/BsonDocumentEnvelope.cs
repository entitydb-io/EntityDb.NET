using EntityDb.Common.Extensions;
using EntityDb.Common.Strategies.Resolving;
using EntityDb.MongoDb.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;

namespace EntityDb.MongoDb.Envelopes
{
    internal sealed record BsonDocumentEnvelope(string? AssemblyFullName, string? TypeFullName, string? TypeName, BsonDocument Value)
    {
        private static BsonDocument GetBsonDocument(dynamic? @object, bool removeTypeDiscriminatorProperty)
        {
            const string typeDiscriminatorPropertyName = "_t";

            var bsonDocument = BsonExtensionMethods.ToBsonDocument(@object, typeof(object));

            if (removeTypeDiscriminatorProperty && bsonDocument.Contains(typeDiscriminatorPropertyName))
            {
                bsonDocument.Remove(typeDiscriminatorPropertyName);
            }

            return bsonDocument;
        }

        public TObject Reconstruct<TObject>(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BsonDocumentEnvelope));

            try
            {
                return (TObject)BsonSerializer.Deserialize(Value, serviceProvider.ResolveType(AssemblyFullName, TypeFullName, TypeName));
            }
            catch (Exception exception)
            {
                logger.LogError(exception.GetHashCode(), exception, "Unable to reconstruct.");

                throw new DeserializeException();
            }
        }

        public byte[] Serialize(Type type, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BsonDocumentEnvelope));

            try
            {
                return this.ToBsonDocument(type).ToBson();
            }
            catch (Exception exception)
            {
                logger.LogError(exception.GetHashCode(), exception, "Unable to serialize.");

                throw new SerializeException();
            }
        }

        public byte[] Serialize(IServiceProvider serviceProvider)
        {
            return Serialize(typeof(BsonDocumentEnvelope), serviceProvider);
        }

        public static BsonDocumentEnvelope Deserialize(byte[] bsonBytes, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BsonDocumentEnvelope));

            try
            {
                var bsonDocument = new RawBsonDocument(bsonBytes);

                return (BsonDocumentEnvelope)BsonSerializer.Deserialize(bsonDocument, typeof(BsonDocumentEnvelope));
            }
            catch (Exception exception)
            {
                logger.LogError(exception.GetHashCode(), exception, "Unable to deserialize.");

                throw new DeserializeException();
            }
        }

        public static BsonDocumentEnvelope Deconstruct(dynamic? @object, IServiceProvider serviceProvider, bool removeTypeDiscriminatorProperty = true)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BsonDocumentEnvelope));

            try
            {
                var bsonDocument = GetBsonDocument(@object, removeTypeDiscriminatorProperty);

                var (assemblyFullName, typeFullName, typeName) = (@object as object)!.GetType().GetTypeInfo();

                return new BsonDocumentEnvelope
                (
                    assemblyFullName,
                    typeFullName,
                    typeName,
                    bsonDocument
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
