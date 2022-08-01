using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Envelopes;

public abstract class EnvelopeTestsBase<TStartup, TEnvelopeValue> : TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    protected EnvelopeTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    protected abstract TEnvelopeValue GenerateCorruptedSerializedData();

    [Fact]
    public void GivenValidRecord_WhenDeconstructedSerializedAndDeserialized_ThenReconstructReturnsEquivalentRecord()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var envelopeService = serviceScope.ServiceProvider
            .GetRequiredService<IEnvelopeService<TEnvelopeValue>>();

        // ACT 

        var record = new TestRecord<bool>(true);

        var boxedRecord = (IRecord)record;

        var envelope = envelopeService.Serialize(boxedRecord);

        var reconstructedBoxedRecord = envelopeService.Deserialize<IRecord>(envelope);

        var reconstructedRecord = (TestRecord<bool>)reconstructedBoxedRecord;

        // ASSERT

        reconstructedRecord.ShouldBeEquivalentTo(record);
    }

    [Fact]
    public void WhenDeserializingCorruptedEnvelope_ThrowDeserializeException()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var envelopeService = serviceScope.ServiceProvider
            .GetRequiredService<IEnvelopeService<TEnvelopeValue>>();

        var corruptedSerializedData = GenerateCorruptedSerializedData();

        // ASSERT

        Should.Throw<DeserializeException>(() => { envelopeService.Deserialize<object>(corruptedSerializedData); });
    }

    [Fact]
    public void WhenSerializingNull_ThrowSerializeException()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var envelopeService = serviceScope.ServiceProvider
            .GetRequiredService<IEnvelopeService<TEnvelopeValue>>();

        // ASSERT

        Should.Throw<SerializeException>(() => { envelopeService.Serialize<object?>(null); });
    }

    private interface IRecord
    {
    }

    protected record TestRecord<TTestProperty>(TTestProperty TestProperty) : IRecord;
}