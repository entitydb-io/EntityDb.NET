using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.Sources.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for a tag query.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface ITagFilterBuilder<TFilter> : IMessageFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags whose <see cref="ITag.Label" /> is
    ///     a particular value.
    /// </summary>
    /// <param name="label">The tag labels</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes tags whose <see cref="ITag.Label" /> is
    ///     <paramref name="label" />.
    /// </returns>
    TFilter TagLabelEq(string label);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes tags whose <see cref="ITag.Value" /> is
    ///     a particular value.
    /// </summary>
    /// <param name="value">The tag values</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes tags whose <see cref="ITag.Value" /> is
    ///     <paramref name="value" />.
    /// </returns>
    TFilter TagValueEq(string value);
}
