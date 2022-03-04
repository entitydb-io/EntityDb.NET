using EntityDb.MongoDb.Envelopes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Tests.Envelopes;
using EntityDb.MongoDb.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using Shouldly;
using Xunit;

namespace EntityDb.MongoDb.Tests.Envelopes;

public class BsonDocumentEnvelopeTests : EnvelopeTestsBase<Startup, BsonDocument>
{
    public BsonDocumentEnvelopeTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    protected override byte[] GenerateCorruptedBytes()
    {
        const string invalidBson = "I AM A STRING VALUE, NOT BSON!";

        return Encoding.UTF8.GetBytes(invalidBson);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenTypeDiscriminatorShouldBeRemovedOption_ThereBsonDocumentMatchesOption(bool removeTypeDiscriminatorProperty)
    {
        // ARRANGE

        var expectedContainsTypeDiscriminatorProperty = !removeTypeDiscriminatorProperty;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.RemoveAll(typeof(IEnvelopeService<BsonDocument>));
            
            serviceCollection.AddBsonDocumentEnvelopeService(removeTypeDiscriminatorProperty);
        });
        
        var envelopeService = serviceScope.ServiceProvider
            .GetRequiredService<IEnvelopeService<BsonDocument>>();

        var value = new TestRecord<bool>(true);

        var bsonDocumentEnvelope = envelopeService.Deconstruct(value);
            
        var actualContainsTypeDiscriminatorProperty = bsonDocumentEnvelope.Value.Contains(BsonDocumentEnvelopeService.TypeDiscriminatorPropertyName);

        // ASSERT

        actualContainsTypeDiscriminatorProperty.ShouldBe(expectedContainsTypeDiscriminatorProperty);
    }
}