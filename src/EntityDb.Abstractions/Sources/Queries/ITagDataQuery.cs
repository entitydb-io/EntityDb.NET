using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Abstractions.Sources.Queries;

/// <summary>
///     Abstracts a query on tags.
/// </summary>
public interface ITagDataQuery : IDataQuery
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> built from a tag filter builder.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    /// <param name="builder">The tag filter builder.</param>
    /// <returns>A <typeparamref name="TFilter" /> built from <paramref name="builder" />.</returns>
    TFilter GetFilter<TFilter>(ITagDataFilterBuilder<TFilter> builder);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> built from a tag sort builder.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The tag sort builder.</param>
    /// <returns>A <typeparamref name="TSort" /> built from <paramref name="builder" />.</returns>
    TSort? GetSort<TSort>(ITagDataSortBuilder<TSort> builder);
}
