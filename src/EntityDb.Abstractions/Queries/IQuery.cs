namespace EntityDb.Abstractions.Queries
{
    /// <summary>
    ///     Abstracts a query for an object repository. Possible objects include: agentSignatures, commands, facts, and leases.
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        ///     The number of objects to skip.
        /// </summary>
        int? Skip { get; }

        /// <summary>
        ///     The number of objects to take.
        /// </summary>
        int? Take { get; }
    }
}
