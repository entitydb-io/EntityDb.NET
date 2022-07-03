using EntityDb.Abstractions.Agents;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Agents;

/// <summary>
///     Represents a type that indicates there is no known actor.
/// </summary>
public class UnknownAgentAccessor : IAgentAccessor
{
    private static readonly Dictionary<string, string> DefaultApplicationInfo = new();

    private readonly IAgentSignatureAugmenter? _agentSignatureAugmenter;

    /// <ignore />
    public UnknownAgentAccessor(IAgentSignatureAugmenter? agentSignatureAugmenter = null)
    {
        _agentSignatureAugmenter = agentSignatureAugmenter;
    }

    /// <inheritdoc />
    public async Task<IAgent> GetAgentAsync(string signatureOptionsName, CancellationToken cancellationToken)
    {
        var applicationInfo = DefaultApplicationInfo;

        if (_agentSignatureAugmenter != null)
        {
            applicationInfo = await _agentSignatureAugmenter.GetApplicationInfoAsync(cancellationToken);
        }

        return new StandardAgent(new UnknownAgentSignature(applicationInfo));
    }
}
