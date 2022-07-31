using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;

namespace EntityDb.Npgsql.Queries;

/// <summary>
///     Defines query options for the Npgsql driver.
/// </summary>
public class NpgsqlQueryOptions
{
    /// <summary>
    ///     Defines the collation for sorting on <see cref="ILease.Value"/>.
    /// </summary>
    public string? LeaseValueSortCollation = null;

    /// <summary>
    ///     Defines teh collation for sorting on <see cref="ITag.Value"/>.
    /// </summary>
    public string? TagValueSortCollation = null;
}
