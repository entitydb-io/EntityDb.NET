namespace EntityDb.Common.Envelopes;

internal interface IEnvelopeService<TEnvelopeValue>
{
    Envelope<TEnvelopeValue> Deconstruct<TData>(TData data);

    byte[] Serialize(Envelope<TEnvelopeValue> envelope);

    Envelope<TEnvelopeValue> Deserialize(byte[] rawData);
    
    TData Reconstruct<TData>(Envelope<TEnvelopeValue> envelope);
}
