using EntityDb.Common.Envelopes;

namespace EntityDb.Common.Extensions;

internal static class EnvelopeServiceExtensions
{
    public static byte[] DeconstructAndSerialize<TEnvelope, TData>(this IEnvelopeService<TEnvelope> envelopeService, TData data)
    {
        var envelope = envelopeService.Deconstruct(data);

        return envelopeService.Serialize(envelope);
    }

    public static TData DeserializeAndReconstruct<TEnvelope, TData>(this IEnvelopeService<TEnvelope> envelopeService,
        byte[] rawData)
    {
        var envelope = envelopeService.Deserialize(rawData);

        return envelopeService.Reconstruct<TData>(envelope);
    }
}
