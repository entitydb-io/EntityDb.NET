using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Exceptions;
using Shouldly;
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

        [Fact]
        public void CanDeconstructAndReconstructGenericBsonDocumentEnvelope()
        {
            TestRecord<bool>? originalTestRecord = new TestRecord<bool>(true);

            IRecord boxedTestRecord = originalTestRecord;

            BsonDocumentEnvelope? bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(boxedTestRecord, _logger);

            byte[]? bsonBytes = bsonDocumentEnvelope.Serialize(_logger);

            BsonDocumentEnvelope? reconstructedBsonDocumentEnvelope =
                BsonDocumentEnvelope.Deserialize(bsonBytes, _logger);

            IRecord? reconstructedTestRecord =
                reconstructedBsonDocumentEnvelope.Reconstruct<IRecord>(_logger, _resolvingStrategyChain);

            TestRecord<bool>? unboxedTestRecord = (TestRecord<bool>)reconstructedTestRecord;

            unboxedTestRecord.TestProperty.ShouldBe(originalTestRecord.TestProperty);
        }

        [Fact]
        public void WhenDeserializingInvalidBsonBytes_ThrowDeserializeException()
        {
            // ARRANGE

            string? invalidBson = "I AM A STRING VALUE, NOT BSON!";

            byte[]? invalidBsonBytes = Encoding.UTF8.GetBytes(invalidBson);

            // ASSERT

            Should.Throw<DeserializeException>(() =>
            {
                BsonDocumentEnvelope.Deserialize(invalidBsonBytes, _logger);
            });
        }

        [Fact]
        public void WhenReconstructingBsonDocumentEnvelopeWithNullValue_ThrowDeserializeException()
        {
            // ARRANGE

            BsonDocumentEnvelope? bsonDocumentEnvelope = new BsonDocumentEnvelope(default!, default!);

            // ASSERT

            Should.Throw<DeserializeException>(() =>
            {
                bsonDocumentEnvelope.Reconstruct<object>(_logger, _resolvingStrategyChain);
            });
        }

        [Fact]
        public void WhenSerializingBsonDocumentEnvelopeWithNullValueAsWrongType_ThrowSerializeException()
        {
            // ARRANGE

            BsonDocumentEnvelope? bsonDocumentEnvelope = new BsonDocumentEnvelope(default!, default!);

            // ASSERT

            Should.Throw<SerializeException>(() =>
            {
                bsonDocumentEnvelope.Serialize(typeof(DateTime), _logger);
            });
        }

        [Fact]
        public void WhenDeconstructingNull_ThrowSerializeException()
        {
            // ASSERT

            Should.Throw<SerializeException>(() =>
            {
                BsonDocumentEnvelope.Deconstruct(default!, _logger);
            });
        }

        [Fact]
        public void GivenTypeDisriminatorShouldBeRemoved_ThereIsNoTypeDiscriminatorInBsonDocument()
        {
            // ARRANGE

            TestRecord<bool>? value = new TestRecord<bool>(true);

            BsonDocumentEnvelope? bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(value, _logger, true);

            // ASSERT

            bsonDocumentEnvelope.Value.Contains(BsonDocumentEnvelope.TypeDiscriminatorPropertyName).ShouldBeFalse();
        }

        [Fact]
        public void GivenTypeDisriminatorShouldNotBeRemoved_ThereIsATypeDiscriminatorInBsonDocument()
        {
            // ARRANGE

            TestRecord<bool>? value = new TestRecord<bool>(true);

            BsonDocumentEnvelope? bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(value, _logger, false);

            // ASSERT

            bsonDocumentEnvelope.Value.Contains(BsonDocumentEnvelope.TypeDiscriminatorPropertyName).ShouldBe(true);
        }

        public interface IRecord
        {
        }

        public record TestRecord<T>(T TestProperty) : IRecord
        {
        }
    }
}
