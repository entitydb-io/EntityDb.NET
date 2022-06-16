using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Agents;

/// <summary>
///     Represents an unknown actor who can interact with transactions.
/// </summary>
public class UnknownAgent : IAgent
{
    /// <inheritdoc/>
    public TimeStamp GetTimeStamp()
    {
        return TimeStamp.UtcNow;
    }

    /// <inheritdoc/>
    public object GetSignature(string signatureOptionsName)
    {
        return new UnknownAgentSignature();
    }
}
