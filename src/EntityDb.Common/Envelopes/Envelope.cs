namespace EntityDb.Common.Envelopes;

/// <summary>
///     Represents an envelope, which can be serialized for transfer.
/// </summary>
/// <param name="Headers">The headers that describe the type of value.</param>
/// <param name="Value">The value, represented in a serializable type.</param>
/// <typeparam name="TEnvelopeValue">The serializable type of the envelope.</typeparam>
public readonly record struct Envelope<TEnvelopeValue>(EnvelopeHeaders Headers, TEnvelopeValue Value);
