namespace EntityDb.Abstractions.Sources.Queries.SortBuilders;

/// <summary>
///     Builds a sort for an object repository. Possible objects include:
///     agentSignatures, deltas, facts, and leases.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface IDataSortBuilder<TSort>
{
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders objects by source timestamp.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders objects by source timestamp.</returns>
    TSort SourceTimeStamp(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders objects by source id.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders objects by source id.</returns>
    TSort SourceId(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders objects by data type.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders objects by data type.</returns>
    TSort DataType(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders objects ordered by a series of sorts.
    /// </summary>
    /// <param name="sorts">The series of sorts.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders objects ordered by <paramref name="sorts" />.</returns>
    TSort Combine(params TSort[] sorts);
}
