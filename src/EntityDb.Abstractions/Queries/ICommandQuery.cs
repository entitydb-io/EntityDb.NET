using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries;

/// <summary>
///     Abstracts a query on commands.
/// </summary>
public interface ICommandQuery : IQuery
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> built from a command filter builder.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    /// <param name="builder">The command filter builder.</param>
    /// <returns>A <typeparamref name="TFilter" /> built from <paramref name="builder" />.</returns>
    TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> built from a command sort builder.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The command sort builder.</param>
    /// <returns>A <typeparamref name="TSort" /> built from <paramref name="builder" />.</returns>
    TSort? GetSort<TSort>(ICommandSortBuilder<TSort> builder);
}
