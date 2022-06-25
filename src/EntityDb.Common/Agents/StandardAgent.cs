using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Agents;

internal sealed record StandardAgent(object Signature) : IAgent
{
    public TimeStamp TimeStamp => TimeStamp.UtcNow;
}
