using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Tests.Implementations.Agents;

public class NoAgent : IAgent
{
    public bool HasRole(string role)
    {
        return false;
    }

    public TimeStamp GetTimeStamp()
    {
        return TimeStamp.UtcNow;
    }

    public object GetSignature(string signatureOptionsName)
    {
        return new NoAgentSignature();
    }
}