using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Exceptions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;

namespace EntityDb.MongoDb.Envelopes
{
    internal sealed record BsonDocumentEnvelope(string? AssemblyFullName, string? TypeFullName, string? TypeName, BsonDocument Value)
    {
        public const string TypeDiscriminatorPropertyName = "_t";

        private static BsonDocument GetBsonDocument(dynamic? @object, bool removeTypeDiscriminatorProperty)
        {
            var bsonDocument = BsonExtensionMethods.ToBsonDocument(@object, typeof(object));

            if (removeTypeDiscriminatorProperty && bsonDocument.Contains(TypeDiscriminatorPropertyName))
            {
                bsonDocument.Remove(TypeDiscriminatorPropertyName);
            }

            return bsonDocument;
        }

        public TObject Reconstruct<TObject>(ILogger logger, IResolvingStrategyChain resolvingStrategyChain)
        {
            try
            {
                return (TObject)BsonSerializer.Deserialize(Value, resolvingStrategyChain.ResolveType(AssemblyFullName, TypeFullName, TypeName));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to reconstruct.");

                throw new DeserializeException();
            }
        }

        public byte[] Serialize(Type type, ILogger logger)
        {
            try
            {
                return this.ToBsonDocument(type).ToBson();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to serialize.");

                throw new SerializeException();
            }
        }

        public byte[] Serialize(ILogger logger)
        {
            return Serialize(typeof(BsonDocumentEnvelope), logger);
        }

        public static BsonDocumentEnvelope Deserialize(byte[] bsonBytes, ILogger logger)
        {
            try
            {
                var bsonDocument = new RawBsonDocument(bsonBytes);

                return (BsonDocumentEnvelope)BsonSerializer.Deserialize(bsonDocument, typeof(BsonDocumentEnvelope));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to deserialize.");

                throw new DeserializeException();
            }
        }

        public static BsonDocumentEnvelope Deconstruct(dynamic? @object, ILogger logger, bool removeTypeDiscriminatorProperty = true)
        {
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
                logger.LogError(exception, "Unable to deconstruct.");

                throw new SerializeException();
            }
        }
    }
}
