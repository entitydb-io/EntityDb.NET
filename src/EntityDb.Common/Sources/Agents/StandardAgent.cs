using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Agents;

internal sealed record StandardAgent(object Signature) : IAgent
{
    public TimeStamp TimeStamp => TimeStamp.UtcNow;
}
