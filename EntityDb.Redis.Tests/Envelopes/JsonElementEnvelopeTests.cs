using EntityDb.Redis.Envelopes;
using EntityDb.Redis.Exceptions;
using System;
using System.Text;
using Xunit;

namespace EntityDb.Redis.Tests.Envelopes
{
    public class JsonElementEnvelopeTests
    {
        private readonly IServiceProvider _serviceProvider;

        public JsonElementEnvelopeTests(IServiceProvider serviceProvider)
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
        public void WhenGoingThroughFullCycle_ThenOriginalMatchesReconstructed()
        {
            // ARRANGE

            var originalTestRecord = new TestRecord<bool>(true);

            IRecord boxedTestRecord = originalTestRecord;

            // ACT

            var jsonElementEnvelope = JsonElementEnvelope.Deconstruct(boxedTestRecord, _serviceProvider);

            var json = jsonElementEnvelope.Serialize(_serviceProvider);

            var reconstructedJsonElementEnvelope = JsonElementEnvelope.Deserialize(json, _serviceProvider);

            var reconstructedTestRecord = reconstructedJsonElementEnvelope.Reconstruct<IRecord>(_serviceProvider);

            var unboxedTestRecord = (TestRecord<bool>)reconstructedTestRecord;

            // ASSERT

            Assert.Equal(originalTestRecord.TestProperty, unboxedTestRecord.TestProperty);
        }

        [Fact]
        public void WhenDeserializingInvalidJsonBytes_ThrowDeserializeException()
        {
            // ARRANGE

            var invalidJson = "I AM A STRING VALUE, NOT JSON!";

            var invalidJsonBytes = Encoding.UTF8.GetBytes(invalidJson);

            // ACT

            Assert.Throws<DeserializeException>(() =>
            {
                JsonElementEnvelope.Deserialize(invalidJsonBytes, _serviceProvider);
            });
        }

        [Fact]
        public void WhenReconstructingJsonElementEnvelopeWithNullValue_ThrowDeserializeException()
        {
            // ARRANGE

            var JsonElementEnvelope = new JsonElementEnvelope(default, default, default, default!);

            // ACT

            Assert.Throws<DeserializeException>(() =>
            {
                JsonElementEnvelope.Reconstruct<object>(_serviceProvider);
            });
        }

        [Fact]
        public void WhenSerializingJsonElementEnvelopeWithNullValue_ThrowSerializeException()
        {
            // ARRANGE

            var JsonElementEnvelope = new JsonElementEnvelope(default, default, default, default!);

            // ACT

            Assert.Throws<SerializeException>(() =>
            {
                JsonElementEnvelope.Serialize(_serviceProvider);
            });
        }

        [Fact]
        public void WhenDeconstructingNull_ThrowSerializeException()
        {
            // ACT

            Assert.Throws<SerializeException>(() =>
            {
                JsonElementEnvelope.Deconstruct(default!, _serviceProvider);
            });
        }
    }
}
