using EntityDb.Abstractions.Queries.Filters;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries
{
    /// <summary>
    ///     Abstracts a query on tags.
    /// </summary>
    public interface ITagQuery : IQuery, ITagFilter
    {
        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> built from a tag sort builder.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="builder">The tag sort builder.</param>
        /// <returns>A <typeparamref name="TSort" /> built from <paramref name="builder" />.</returns>
        TSort? GetSort<TSort>(ITagSortBuilder<TSort> builder);
    }
}
