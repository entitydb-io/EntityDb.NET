using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Abstractions.Sources.Queries.SortBuilders;

/// <summary>
///     Builds a sort for a tag query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface ITagSortBuilder<TSort> : IMessageDataSortBuilder<TSort>
{
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by <see cref="ITag.Label" />.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by <see cref="ITag.Label" />.</returns>
    TSort TagLabel(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders tags by <see cref="ITag.Value" />.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders tags by <see cref="ITag.Value" />.</returns>
    TSort TagValue(bool ascending);
}
