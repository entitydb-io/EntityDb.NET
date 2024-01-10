namespace EntityDb.Abstractions.Sources.Queries.SortBuilders;

/// <summary>
///     Builds a <typeparamref name="TSort" /> for a message query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface IMessageDataSortBuilder<TSort> : ISortBuilder<TSort>
{
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders objects by state id.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders objects by state id.</returns>
    TSort StateId(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders objects by state version.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders objects by state version.</returns>
    TSort StateVersion(bool ascending);
}
