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

internal class BsonDocumentEnvelopeService : IEnvelopeService<BsonDocument>
{
    public const string TypeDiscriminatorPropertyName = "_t";

    private readonly ILogger<BsonDocumentEnvelopeService> _logger;
    private readonly ITypeResolver _typeResolver;
    private readonly bool _removeTypeDiscriminatorProperty;

    static BsonDocumentEnvelopeService()
    {
        BsonSerializer.RegisterSerializer(new EnvelopeSerializer());
    }

    public BsonDocumentEnvelopeService
    (
        ILogger<BsonDocumentEnvelopeService> logger,
        ITypeResolver typeResolver,
        bool removeTypeDiscriminatorProperty
    )
    {
        _logger = logger;
        _typeResolver = typeResolver;
        _removeTypeDiscriminatorProperty = removeTypeDiscriminatorProperty;
    }

    public Envelope<BsonDocument> Deconstruct<TData>(TData data)
    {
        try
        {
            var bsonDocument = data.ToBsonDocument(typeof(object));

            if (_removeTypeDiscriminatorProperty && bsonDocument.Contains(TypeDiscriminatorPropertyName))
            {
                bsonDocument.Remove(TypeDiscriminatorPropertyName);
            }

            var headers = EnvelopeHelper.GetEnvelopeHeaders(data!.GetType());

            return new Envelope<BsonDocument>(headers, bsonDocument);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to deconstruct");

            throw new SerializeException();
        }
    }

    public byte[] Serialize(Envelope<BsonDocument> envelope)
    {
        try
        {
            return envelope.ToBsonDocument().ToBson();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to serialize");

            throw new SerializeException();
        }
    }

    public Envelope<BsonDocument> Deserialize(byte[] rawData)
    {
        try
        {
            var bsonDocument = new RawBsonDocument(rawData);

            return (Envelope<BsonDocument>)BsonSerializer.Deserialize(bsonDocument, typeof(Envelope<BsonDocument>));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to deserialize");

            throw new DeserializeException();
        }
    }

    public TData Reconstruct<TData>(Envelope<BsonDocument> envelope)
    {
        try
        {
            return (TData)BsonSerializer.Deserialize(envelope.Value, _typeResolver.ResolveType(envelope.Headers));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to reconstruct");

            throw new DeserializeException();
        }
    }

    public static IEnvelopeService<BsonDocument> Create(IServiceProvider serviceProvider,
        bool removeTypeDiscriminatorProperty)
    {
        return ActivatorUtilities.CreateInstance<BsonDocumentEnvelopeService>(serviceProvider,
            removeTypeDiscriminatorProperty);
    }
}
