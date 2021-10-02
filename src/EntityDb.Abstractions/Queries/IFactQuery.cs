using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries
{
    /// <summary>
    /// Abstracts a query on facts.
    /// </summary>
    public interface IFactQuery : IQuery
    {
        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> built from a fact filter builder.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="builder">The fact filter builder.</param>
        /// <returns>A <typeparamref name="TFilter"/> built from <paramref name="builder"/>.</returns>
        TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> built from a fact sort builder.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="builder">The fact sort builder.</param>
        /// <returns>A <typeparamref name="TSort"/> built from <paramref name="builder"/>.</returns>
        TSort? GetSort<TSort>(IFactSortBuilder<TSort> builder);
    }
}
