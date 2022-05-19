using System.Collections.Generic;

namespace EntityDb.Abstractions.Agents;

/// <summary>
///     Represents a type that can augment an agent signature by
///     providing additional, application-specific information.
/// </summary>
public interface IAgentSignatureAugmenter
{
    /// <summary>
    ///     Returns a dictionary of application-specific information.
    /// </summary>
    /// <returns>A dictionary of application-specific information.</returns>
    Dictionary<string, string> GetApplicationInfo();
}
