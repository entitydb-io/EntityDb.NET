namespace EntityDb.Abstractions.Sources.Queries.SortBuilders;

/// <summary>
///     Builds a <typeparamref name="TSort" /> for a source message query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface IMessageSortBuilder<TSort> : ISortBuilder<TSort>
{
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders objects by entity id.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders objects by entity id.</returns>
    TSort EntityId(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders objects by entity version.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders objects by entity version.</returns>
    TSort EntityVersion(bool ascending);
}
