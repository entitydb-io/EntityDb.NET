using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Abstractions.Sources.Queries;

/// <summary>
///     Abstracts a query on messages.
/// </summary>
public interface IMessageDataQuery : IQuery
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> built from a message filter builder.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    /// <param name="builder">The message filter builder.</param>
    /// <returns>A <typeparamref name="TFilter" /> built from <paramref name="builder" />.</returns>
    TFilter GetFilter<TFilter>(IMessageDataFilterBuilder<TFilter> builder);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> built from a message sort builder.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The message sort builder.</param>
    /// <returns>A <typeparamref name="TSort" /> built from <paramref name="builder" />.</returns>
    TSort? GetSort<TSort>(IMessageDataSortBuilder<TSort> builder);
}
