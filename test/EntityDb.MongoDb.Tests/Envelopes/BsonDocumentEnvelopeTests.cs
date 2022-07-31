using System;
using System.Text;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Tests.Envelopes;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Extensions;
using Microsoft.Extensions.DependencyInjection;
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

    protected override BsonDocument GenerateCorruptedSerializedData()
    {
        return null!;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenTypeDiscriminatorShouldBeRemovedOption_ThereBsonDocumentMatchesOption(
        bool removeTypeDiscriminatorProperty)
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

        var bsonDocumentEnvelope = envelopeService.Serialize(value);

        var actualContainsTypeDiscriminatorProperty =
            bsonDocumentEnvelope.GetElement("Value").Value.AsBsonDocument.Contains(MongoDbEnvelopeService.TypeDiscriminatorPropertyName);

        // ASSERT

        actualContainsTypeDiscriminatorProperty.ShouldBe(expectedContainsTypeDiscriminatorProperty);
    }
}