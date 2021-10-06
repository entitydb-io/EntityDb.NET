using EntityDb.Abstractions.Queries.Filters;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries
{
    /// <summary>
    ///     Abstracts a query on leases.
    /// </summary>
    public interface ILeaseQuery : IQuery, ILeaseFilter
    {
        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> built from a lease sort builder.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="builder">The lease sort builder.</param>
        /// <returns>A <typeparamref name="TSort" /> built from <paramref name="builder" />.</returns>
        TSort? GetSort<TSort>(ILeaseSortBuilder<TSort> builder);
    }
}
