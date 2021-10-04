using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.FilterBuilders
{
    /// <summary>
    ///     Builds a <typeparamref name="TFilter" /> for a lease query.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    public interface ILeaseFilterBuilder<TFilter> : IFilterBuilder<TFilter>
    {
        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that only includes leases with an entity id which is contained in a set
        ///     of entity ids.
        /// </summary>
        /// <param name="entityIds">The set of entity ids.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that only includes leases with an entity id which is contained in
        ///     <paramref name="entityIds" />.
        /// </returns>
        TFilter EntityIdIn(params Guid[] entityIds);

        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that only includes leases with an entity version number greater than or
        ///     equal to an entity version number.
        /// </summary>
        /// <param name="entityVersionNumber">The entity version number.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that only includes leases with an entity version number greater than or
        ///     equal to <paramref name="entityVersionNumber" />.
        /// </returns>
        TFilter EntityVersionNumberGte(ulong entityVersionNumber);

        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that only includes leases with an entity version number less than or
        ///     equal to an entity version number.
        /// </summary>
        /// <param name="entityVersionNumber">The entity version number.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that only includes leases with an entity version number less than or equal
        ///     to <paramref name="entityVersionNumber" />.
        /// </returns>
        TFilter EntityVersionNumberLte(ulong entityVersionNumber);

        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that only includes leases whose type is contained in a set of lease
        ///     types.
        /// </summary>
        /// <param name="leaseTypes">The lease types.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that only includes leases whose type is contained in
        ///     <paramref name="leaseTypes" />.
        /// </returns>
        TFilter LeaseTypeIn(params Type[] leaseTypes);

        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> that only includes leases which do match a lease expression.
        /// </summary>
        /// <typeparam name="TLease">The type of lease in the lease expression.</typeparam>
        /// <param name="leaseExpression">The lease expression.</param>
        /// <returns>
        ///     A <typeparamref name="TFilter" /> that only includes leases which do match <paramref name="leaseExpression" />
        ///     .
        /// </returns>
        TFilter LeaseMatches<TLease>(Expression<Func<TLease, bool>> leaseExpression);
    }
}
