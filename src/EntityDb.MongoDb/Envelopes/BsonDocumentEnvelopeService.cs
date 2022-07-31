using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using EntityDb.Common.TypeResolvers;
using EntityDb.MongoDb.Serializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;

namespace EntityDb.MongoDb.Envelopes;

internal class MongoDbEnvelopeService : IEnvelopeService<BsonDocument>
{
    public const string TypeDiscriminatorPropertyName = "_t";

    private readonly ILogger<MongoDbEnvelopeService> _logger;
    private readonly bool _removeTypeDiscriminatorProperty;
    private readonly ITypeResolver _typeResolver;

    static MongoDbEnvelopeService()
    {
        BsonSerializer.RegisterSerializer(new EnvelopeSerializer());
    }

    public MongoDbEnvelopeService
    (
        ILogger<MongoDbEnvelopeService> logger,
        ITypeResolver typeResolver,
        bool removeTypeDiscriminatorProperty
    )
    {
        _logger = logger;
        _typeResolver = typeResolver;
        _removeTypeDiscriminatorProperty = removeTypeDiscriminatorProperty;
    }

    public BsonDocument Serialize<TData>(TData data)
    {
        try
        {
            var bsonDocument = data.ToBsonDocument(typeof(object));

            if (_removeTypeDiscriminatorProperty && bsonDocument.Contains(TypeDiscriminatorPropertyName))
            {
                bsonDocument.Remove(TypeDiscriminatorPropertyName);
            }

            var headers = EnvelopeHelper.GetEnvelopeHeaders(data!.GetType());

            var envelope = new Envelope<BsonDocument>(headers, bsonDocument);

            return envelope.ToBsonDocument();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to serialize");

            throw new SerializeException();
        }
    }

    public TData Deserialize<TData>(BsonDocument serializedData)
    {
        try
        {
            var envelope = (Envelope<BsonDocument>)BsonSerializer.Deserialize(serializedData, typeof(Envelope<BsonDocument>));

            return (TData)BsonSerializer.Deserialize(envelope.Value, _typeResolver.ResolveType(envelope.Headers));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to deserialize");

            throw new DeserializeException();
        }
    }

    public static IEnvelopeService<BsonDocument> Create(IServiceProvider serviceProvider,
        bool removeTypeDiscriminatorProperty)
    {
        return ActivatorUtilities.CreateInstance<MongoDbEnvelopeService>(serviceProvider,
            removeTypeDiscriminatorProperty);
    }
}
