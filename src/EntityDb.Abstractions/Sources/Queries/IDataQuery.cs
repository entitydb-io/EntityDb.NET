namespace EntityDb.Abstractions.Sources.Queries;

/// <summary>
///     Abstracts a query for an object repository. Possible objects include:
///     agentSignatures, deltas, facts, and leases.
/// </summary>
public interface IDataQuery
{
    /// <summary>
    ///     The number of objects to skip.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    ///     The number of objects to take.
    /// </summary>
    int? Take { get; }

    /// <summary>
    ///     Driver-specific options for this query
    /// </summary>
    object? Options { get; }
}
