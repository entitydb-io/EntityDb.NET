using System;
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

    protected abstract byte[] GenerateCorruptedBytes();
    
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

        var envelope = envelopeService.Deconstruct(boxedRecord);

        var rawData = envelopeService.Serialize(envelope);

        var reconstructedEnvelope = envelopeService.Deserialize(rawData);
        
        var reconstructedBoxedRecord = envelopeService.Reconstruct<IRecord>(reconstructedEnvelope);

        var reconstructedRecord = (TestRecord<bool>)reconstructedBoxedRecord;

        // ASSERT

        reconstructedRecord.ShouldBeEquivalentTo(record);
    }
    
    [Fact]
    public void WhenSerializingCorruptedEnvelope_ThrowSerializeException()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var envelopeService = serviceScope.ServiceProvider
            .GetRequiredService<IEnvelopeService<TEnvelopeValue>>();

        var envelope = new Envelope<TEnvelopeValue>(default!, default!);

        // ACT

        Should.Throw<SerializeException>(() =>
        {
            envelopeService.Serialize(envelope);
        });
    }
    
    [Fact]
    public void WhenDeserializingCorruptedBytes_ThrowDeserializeException()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var envelopeService = serviceScope.ServiceProvider
            .GetRequiredService<IEnvelopeService<TEnvelopeValue>>();

        var corruptedBytes = GenerateCorruptedBytes();

        // ASSERT

        Should.Throw<DeserializeException>(() =>
        {
            envelopeService.Deserialize(corruptedBytes);
        });
    }

    [Fact]
    public void WhenReconstructingCorruptedEnvelope_ThrowDeserializeException()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var envelopeService = serviceScope.ServiceProvider
            .GetRequiredService<IEnvelopeService<TEnvelopeValue>>();

        var envelope = new Envelope<TEnvelopeValue>(default!, default!);

        // ASSERT

        Should.Throw<DeserializeException>(() =>
        {
            envelopeService.Reconstruct<object>(envelope);
        });
    }

    [Fact]
    public void WhenDeconstructingNull_ThrowSerializeException()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var envelopeService = serviceScope.ServiceProvider
            .GetRequiredService<IEnvelopeService<TEnvelopeValue>>();

        // ASSERT

        Should.Throw<SerializeException>(() =>
        {
            envelopeService.Deconstruct<object?>(null);
        });
    }

    private interface IRecord
    {
    }

    protected record TestRecord<TTestProperty>(TTestProperty TestProperty) : IRecord;
}