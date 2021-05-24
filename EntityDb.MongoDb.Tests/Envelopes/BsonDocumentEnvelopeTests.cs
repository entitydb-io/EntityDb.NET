using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Exceptions;
using System;
using System.Text;
using Xunit;

namespace EntityDb.MongoDb.Tests.Envelopes
{
    public class BsonDocumentEnvelopeTests
    {
        private readonly IServiceProvider _serviceProvider;

        public BsonDocumentEnvelopeTests(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(boxedTestRecord, _serviceProvider);

            var bsonBytes = bsonDocumentEnvelope.Serialize(_serviceProvider);

            var reconstructedBsonDocumentEnvelope = BsonDocumentEnvelope.Deserialize(bsonBytes, _serviceProvider);

            var reconstructedTestRecord = reconstructedBsonDocumentEnvelope.Reconstruct<IRecord>(_serviceProvider);

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
                BsonDocumentEnvelope.Deserialize(invalidBsonBytes, _serviceProvider);
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
                bsonDocumentEnvelope.Reconstruct<object>(_serviceProvider);
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
                bsonDocumentEnvelope.Serialize(typeof(DateTime), _serviceProvider);
            });
        }

        [Fact]
        public void WhenDeconstructingNull_ThrowSerializeException()
        {
            // ASSERT

            Assert.Throws<SerializeException>(() =>
            {
                BsonDocumentEnvelope.Deconstruct(default!, _serviceProvider);
            });
        }

        [Fact]
        public void GivenTypeDisriminatorShouldBeRemoved_ThereIsNoTypeDiscriminatorInBsonDocument()
        {
            // ARRANGE

            var value = new TestRecord<bool>(true);

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(value, _serviceProvider, removeTypeDiscriminatorProperty: true);

            // ASSERT

            Assert.False(bsonDocumentEnvelope.Value.Contains("_t"));
        }

        [Fact]
        public void GivenTypeDisriminatorShouldNotBeRemoved_ThereIsATypeDiscriminatorInBsonDocument()
        {
            // ARRANGE

            var value = new TestRecord<bool>(true);

            var bsonDocumentEnvelope = BsonDocumentEnvelope.Deconstruct(value, _serviceProvider, removeTypeDiscriminatorProperty: false);

            // ASSERT

            Assert.True(bsonDocumentEnvelope.Value.Contains("_t"));
        }
    }
}
