using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries
{
    /// <summary>
    /// Abstracts a query on sources.
    /// </summary>
    public interface ISourceQuery : IQuery
    {
        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> built from a source filter builder.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="builder">The source filter builder.</param>
        /// <returns>A <typeparamref name="TFilter"/> built from <paramref name="builder"/>.</returns>
        TFilter GetFilter<TFilter>(ISourceFilterBuilder<TFilter> builder);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> built from a source sort builder.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="builder">The source sort builder.</param>
        /// <returns>A <typeparamref name="TSort"/> built from <paramref name="builder"/>.</returns>
        TSort? GetSort<TSort>(ISourceSortBuilder<TSort> builder);
    }
}
