using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries
{
    /// <summary>
    /// Abstracts a query on leases.
    /// </summary>
    public interface ILeaseQuery : IQuery
    {
        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> built from a lease filter builder.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="builder">The lease filter builder.</param>
        /// <returns>A <typeparamref name="TFilter"/> built from <paramref name="builder"/>.</returns>
        TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> built from a lease sort builder.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="builder">The lease sort builder.</param>
        /// <returns>A <typeparamref name="TSort"/> built from <paramref name="builder"/>.</returns>
        TSort? GetSort<TSort>(ILeaseSortBuilder<TSort> builder);
    }
}
