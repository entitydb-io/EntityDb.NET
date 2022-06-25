using System.Threading.Tasks;

namespace EntityDb.Abstractions.Agents;

/// <summary>
///     Represents a type that can access an instance of <see cref="IAgent" />.
/// </summary>
public interface IAgentAccessor
{
    /// <summary>
    ///     Returns the agent.
    /// </summary>
    /// <param name="signatureOptionsName">The name of the signature options object.</param>
    /// <returns>The agent.</returns>
    Task<IAgent> GetAgentAsync(string signatureOptionsName);
}
