namespace EntityDb.Common.Envelopes;

internal interface IEnvelopeService<TSerializedData>
{
    TSerializedData Serialize<TData>(TData data);

    TData Deserialize<TData>(TSerializedData serializedData);
}
