using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Agents;

/// <summary>
///     Represents a type that can access an instance of <see cref="IAgent" /> within a service scope.
/// </summary>
public interface IAgentAccessor
{
    /// <summary>
    ///     Returns the agent of the service scope.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The agent of the service scope.</returns>
    Task<IAgent> GetAgentAsync(CancellationToken cancellationToken = default);
}
