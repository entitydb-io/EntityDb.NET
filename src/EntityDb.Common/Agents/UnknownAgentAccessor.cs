using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.ValueObjects;
using System.Threading.Tasks;

namespace EntityDb.Common.Agents;

/// <summary>
///     Represents a type that indicates there is no known actor.
/// </summary>
public class UnknownAgentAccessor : IAgentAccessor
{
    /// <inheritdoc/>
    public Task<IAgent> GetAgentAsync(string signatureOptionsName)
    {
        return Task.FromResult<IAgent>(new StandardAgent(new UnknownAgentSignature()));
    }
}
