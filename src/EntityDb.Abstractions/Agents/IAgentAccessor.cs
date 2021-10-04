namespace EntityDb.Abstractions.Agents
{
    /// <summary>
    ///     Represents a type that can access an instance of <see cref="IAgent" /> within a service scope.
    /// </summary>
    public interface IAgentAccessor
    {
        /// <summary>
        ///     Returns the agent of the service scope.
        /// </summary>
        /// <returns>The agent of the service scope.</returns>
        IAgent GetAgent();
    }
}
