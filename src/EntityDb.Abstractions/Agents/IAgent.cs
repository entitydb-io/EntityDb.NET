namespace EntityDb.Abstractions.Agents
{
    /// <summary>
    ///     Represents an actor who can interact with transactions.
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        ///     Returns wether or not the agent has a particular role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns>Wether or not the agent has a particular role.</returns>
        bool HasRole(string role);

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        object GetSignature();
    }
}
