using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Extensions;
using EntityDb.Redis.Envelopes;
using EntityDb.Redis.Exceptions;
using Shouldly;
using System.Text;
using Xunit;

namespace EntityDb.Redis.Tests.Envelopes
{
    public class JsonElementEnvelopeTests
    {
        private readonly ILogger _logger;
        private readonly IResolvingStrategyChain _resolvingStrategyChain;

        public JsonElementEnvelopeTests(ILoggerFactory loggerFactory, IResolvingStrategyChain resolvingStrategyChain)
        {
            _logger = loggerFactory.CreateLogger<JsonElementEnvelopeTests>();
            _resolvingStrategyChain = resolvingStrategyChain;
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

            var jsonElementEnvelope = JsonElementEnvelope.Deconstruct(boxedTestRecord, _logger);

            var json = jsonElementEnvelope.Serialize(_logger);

            var reconstructedJsonElementEnvelope = JsonElementEnvelope.Deserialize(json, _logger);

            var reconstructedTestRecord = reconstructedJsonElementEnvelope.Reconstruct<IRecord>(_logger, _resolvingStrategyChain);

            var unboxedTestRecord = (TestRecord<bool>)reconstructedTestRecord;

            // ASSERT

            unboxedTestRecord.TestProperty.ShouldBe(originalTestRecord.TestProperty);
        }

        [Fact]
        public void WhenDeserializingInvalidJsonBytes_ThrowDeserializeException()
        {
            // ARRANGE

            var invalidJson = "I AM A STRING VALUE, NOT JSON!";

            var invalidJsonBytes = Encoding.UTF8.GetBytes(invalidJson);

            // ACT

            Should.Throw<DeserializeException>(() =>
            {
                JsonElementEnvelope.Deserialize(invalidJsonBytes, _logger);
            });
        }

        [Fact]
        public void WhenReconstructingJsonElementEnvelopeWithNullValue_ThrowDeserializeException()
        {
            // ARRANGE

            var JsonElementEnvelope = new JsonElementEnvelope(default, default, default, default!);

            // ACT

            Should.Throw<DeserializeException>(() =>
            {
                JsonElementEnvelope.Reconstruct<object>(_logger, _resolvingStrategyChain);
            });
        }

        [Fact]
        public void WhenSerializingJsonElementEnvelopeWithNullValue_ThrowSerializeException()
        {
            // ARRANGE

            var JsonElementEnvelope = new JsonElementEnvelope(default, default, default, default!);

            // ACT

            Should.Throw<SerializeException>(() =>
            {
                JsonElementEnvelope.Serialize(_logger);
            });
        }

        [Fact]
        public void WhenDeconstructingNull_ThrowSerializeException()
        {
            // ACT

            Should.Throw<SerializeException>(() =>
            {
                JsonElementEnvelope.Deconstruct(default!, _logger);
            });
        }
    }
}
