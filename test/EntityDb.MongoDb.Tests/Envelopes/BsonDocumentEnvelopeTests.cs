using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.Common.Tests;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Text;
using Xunit;

namespace EntityDb.MongoDb.Tests.Envelopes
{
    public class BsonDocumentEnvelopeTests : TestsBase<Startup>
    {
        public BsonDocumentEnvelopeTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }

        [Fact]
        public void CanDeconstructAndReconstructGenericBsonDocumentEnvelope()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<BsonDocumentEnvelopeTests>();

            var resolvingStategyChain = serviceScope.ServiceProvider
                .GetRequiredService<IResolvingStrategyChain>();

            // ACT 

            var originalTestRecord = new TestRecord<bool>(true);

            IRecord boxedTestRecord = originalTestRecord;

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(boxedTestRecord, logger);

            var bsonBytes = bsonDocumentEnvelope.Serialize(logger);

            var reconstructedBsonDocumentEnvelope = BsonDocumentEnvelope.Deserialize(bsonBytes, logger);

            var reconstructedTestRecord =
                reconstructedBsonDocumentEnvelope.Reconstruct<IRecord>(logger, resolvingStategyChain);

            var unboxedTestRecord = (TestRecord<bool>)reconstructedTestRecord;

            // ASSERT

            unboxedTestRecord.TestProperty.ShouldBe(originalTestRecord.TestProperty);
        }

        [Fact]
        public void WhenDeserializingInvalidBsonBytes_ThrowDeserializeException()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<BsonDocumentEnvelopeTests>();

            var invalidBson = "I AM A STRING VALUE, NOT BSON!";

            var invalidBsonBytes = Encoding.UTF8.GetBytes(invalidBson);

            // ASSERT

            Should.Throw<DeserializeException>(() =>
            {
                BsonDocumentEnvelope.Deserialize(invalidBsonBytes, logger);
            });
        }

        [Fact]
        public void WhenReconstructingBsonDocumentEnvelopeWithNullValue_ThrowDeserializeException()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<BsonDocumentEnvelopeTests>();

            var resolvingStategyChain = serviceScope.ServiceProvider
                .GetRequiredService<IResolvingStrategyChain>();

            var bsonDocumentEnvelope = new BsonDocumentEnvelope(default!, default!);

            // ASSERT

            Should.Throw<DeserializeException>(() =>
            {
                bsonDocumentEnvelope.Reconstruct<object>(logger, resolvingStategyChain);
            });
        }

        [Fact]
        public void WhenSerializingBsonDocumentEnvelopeWithNullValueAsWrongType_ThrowSerializeException()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<BsonDocumentEnvelopeTests>();

            var bsonDocumentEnvelope = new BsonDocumentEnvelope(default!, default!);

            // ASSERT

            Should.Throw<SerializeException>(() =>
            {
                bsonDocumentEnvelope.Serialize(typeof(DateTime), logger);
            });
        }

        [Fact]
        public void WhenDeconstructingNull_ThrowSerializeException()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<BsonDocumentEnvelopeTests>();

            // ASSERT

            Should.Throw<SerializeException>(() =>
            {
                BsonDocumentEnvelope.Deconstruct(default!, logger);
            });
        }

        [Fact]
        public void GivenTypeDisriminatorShouldBeRemoved_ThereIsNoTypeDiscriminatorInBsonDocument()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<BsonDocumentEnvelopeTests>();

            var value = new TestRecord<bool>(true);

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(value, logger);

            // ASSERT

            bsonDocumentEnvelope.Value.Contains(BsonDocumentEnvelope.TypeDiscriminatorPropertyName).ShouldBeFalse();
        }

        [Fact]
        public void GivenTypeDisriminatorShouldNotBeRemoved_ThereIsATypeDiscriminatorInBsonDocument()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<BsonDocumentEnvelopeTests>();

            var value = new TestRecord<bool>(true);

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(value, logger, false);

            // ASSERT

            bsonDocumentEnvelope.Value.Contains(BsonDocumentEnvelope.TypeDiscriminatorPropertyName).ShouldBe(true);
        }

        public interface IRecord
        {
        }

        public record TestRecord<T>(T TestProperty) : IRecord;
    }
}
