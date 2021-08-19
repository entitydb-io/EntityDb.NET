using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Exceptions;
using System;
using System.Text;
using Xunit;

namespace EntityDb.MongoDb.Tests.Envelopes
{
    public class BsonDocumentEnvelopeTests
    {
        private readonly ILogger _logger;
        private readonly IResolvingStrategyChain _resolvingStrategyChain;

        public BsonDocumentEnvelopeTests(ILoggerFactory loggerFactory, IResolvingStrategyChain resolvingStrategyChain)
        {
            _logger = loggerFactory.CreateLogger<BsonDocumentEnvelopeTests>();
            _resolvingStrategyChain = resolvingStrategyChain;
        }

        public interface IRecord
        {
        }

        public record TestRecord<T>(T TestProperty) : IRecord
        {
        }

        [Fact]
        public void CanDeconstructAndReconstructGenericBsonDocumentEnvelope()
        {
            var originalTestRecord = new TestRecord<bool>(true);

            IRecord boxedTestRecord = originalTestRecord;

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(boxedTestRecord, _logger);

            var bsonBytes = bsonDocumentEnvelope.Serialize(_logger);

            var reconstructedBsonDocumentEnvelope = BsonDocumentEnvelope.Deserialize(bsonBytes, _logger);

            var reconstructedTestRecord = reconstructedBsonDocumentEnvelope.Reconstruct<IRecord>(_logger, _resolvingStrategyChain);

            var unboxedTestRecord = (TestRecord<bool>)reconstructedTestRecord;

            Assert.Equal(originalTestRecord.TestProperty, unboxedTestRecord.TestProperty);
        }

        [Fact]
        public void WhenDeserializingInvalidBsonBytes_ThrowDeserializeException()
        {
            // ARRANGE

            var invalidBson = "I AM A STRING VALUE, NOT BSON!";

            var invalidBsonBytes = Encoding.UTF8.GetBytes(invalidBson);

            // ASSERT

            Assert.Throws<DeserializeException>(() =>
            {
                BsonDocumentEnvelope.Deserialize(invalidBsonBytes, _logger);
            });
        }

        [Fact]
        public void WhenReconstructingBsonDocumentEnvelopeWithNullValue_ThrowDeserializeException()
        {
            // ARRANGE

            var bsonDocumentEnvelope = new BsonDocumentEnvelope(default!, default!, default!, default!);

            // ASSERT

            Assert.Throws<DeserializeException>(() =>
            {
                bsonDocumentEnvelope.Reconstruct<object>(_logger, _resolvingStrategyChain);
            });
        }

        [Fact]
        public void WhenSerializingBsonDocumentEnvelopeWithNullValueAsWrongType_ThrowSerializeException()
        {
            // ARRANGE

            var bsonDocumentEnvelope = new BsonDocumentEnvelope(default!, default!, default!, default!);

            // ASSERT

            Assert.Throws<SerializeException>(() =>
            {
                bsonDocumentEnvelope.Serialize(typeof(DateTime), _logger);
            });
        }

        [Fact]
        public void WhenDeconstructingNull_ThrowSerializeException()
        {
            // ASSERT

            Assert.Throws<SerializeException>(() =>
            {
                BsonDocumentEnvelope.Deconstruct(default!, _logger);
            });
        }

        [Fact]
        public void GivenTypeDisriminatorShouldBeRemoved_ThereIsNoTypeDiscriminatorInBsonDocument()
        {
            // ARRANGE

            var value = new TestRecord<bool>(true);

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(value, _logger, removeTypeDiscriminatorProperty: true);

            // ASSERT

            Assert.False(bsonDocumentEnvelope.Value.Contains("_t"));
        }

        [Fact]
        public void GivenTypeDisriminatorShouldNotBeRemoved_ThereIsATypeDiscriminatorInBsonDocument()
        {
            // ARRANGE

            var value = new TestRecord<bool>(true);

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(value, _logger, removeTypeDiscriminatorProperty: false);

            // ASSERT

            Assert.True(bsonDocumentEnvelope.Value.Contains("_t"));
        }
    }
}
