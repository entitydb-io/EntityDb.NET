using EntityDb.Abstractions.Tags;

namespace EntityDb.Abstractions.Queries.SortBuilders;

/// <summary>
///     Builds a sort for a tag query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface ITagSortBuilder<TSort> : ISortBuilder<TSort>
{
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by entity id.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by entity id.</returns>
    TSort EntityId(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by entity version number.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by entity version number.</returns>
    TSort EntityVersionNumber(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by type.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by type.</returns>
    TSort TagType(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by <see cref="ITag.Label"/>.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by <see cref="ITag.Label"/>.</returns>
    TSort TagLabel(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by <see cref="ITag.Value"/>.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by <see cref="ITag.Value"/>.</returns>
    TSort TagValue(bool ascending);
}
