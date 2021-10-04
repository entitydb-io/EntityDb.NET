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

        [Fact]
        public void WhenGoingThroughFullCycle_ThenOriginalMatchesReconstructed()
        {
            // ARRANGE

            TestRecord<bool>? originalTestRecord = new TestRecord<bool>(true);

            IRecord boxedTestRecord = originalTestRecord;

            // ACT

            JsonElementEnvelope? jsonElementEnvelope = JsonElementEnvelope.Deconstruct(boxedTestRecord, _logger);

            byte[]? json = jsonElementEnvelope.Serialize(_logger);

            JsonElementEnvelope? reconstructedJsonElementEnvelope = JsonElementEnvelope.Deserialize(json, _logger);

            IRecord? reconstructedTestRecord =
                reconstructedJsonElementEnvelope.Reconstruct<IRecord>(_logger, _resolvingStrategyChain);

            TestRecord<bool>? unboxedTestRecord = (TestRecord<bool>)reconstructedTestRecord;

            // ASSERT

            unboxedTestRecord.TestProperty.ShouldBe(originalTestRecord.TestProperty);
        }

        [Fact]
        public void WhenDeserializingInvalidJsonBytes_ThrowDeserializeException()
        {
            // ARRANGE

            string? invalidJson = "I AM A STRING VALUE, NOT JSON!";

            byte[]? invalidJsonBytes = Encoding.UTF8.GetBytes(invalidJson);

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

            JsonElementEnvelope? jsonElementEnvelope = new JsonElementEnvelope();

            // ACT

            Should.Throw<DeserializeException>(() =>
            {
                jsonElementEnvelope.Reconstruct<object>(_logger, _resolvingStrategyChain);
            });
        }

        [Fact]
        public void WhenSerializingJsonElementEnvelopeWithNullValue_ThrowSerializeException()
        {
            // ARRANGE

            JsonElementEnvelope? jsonElementEnvelope = new JsonElementEnvelope();

            // ACT

            Should.Throw<SerializeException>(() =>
            {
                jsonElementEnvelope.Serialize(_logger);
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

        public interface IRecord
        {
        }

        public record TestRecord<T>(T TestProperty) : IRecord
        {
        }
    }
}
