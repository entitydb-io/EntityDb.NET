using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;

namespace EntityDb.Common.Sources.Agents;

internal sealed record StandardAgent(object Signature) : IAgent
{
    public TimeStamp TimeStamp => TimeStamp.UtcNow;
}
