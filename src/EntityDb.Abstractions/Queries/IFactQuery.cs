using EntityDb.Abstractions.Queries.Filters;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries
{
    /// <summary>
    ///     Abstracts a query on facts.
    /// </summary>
    public interface IFactQuery : IQuery, IFactFilter
    {
        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> built from a fact sort builder.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="builder">The fact sort builder.</param>
        /// <returns>A <typeparamref name="TSort" /> built from <paramref name="builder" />.</returns>
        TSort? GetSort<TSort>(IFactSortBuilder<TSort> builder);
    }
}
