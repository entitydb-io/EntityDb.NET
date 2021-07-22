using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.SortBuilders
{
    /// <summary>
    /// Builds a sort for a lease repository.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    public interface ILeaseSortBuilder<TSort> : ISortBuilder<TSort>
    {
        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders leases by entity id.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders leases by entity id.</returns>
        TSort EntityId(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders leases by entity version number.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders leases by entity version number.</returns>
        TSort EntityVersionNumber(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders leases by type.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort"/> that orders leases by type.</returns>
        TSort LeaseType(bool ascending);

        /// <summary>
        /// Returns a <typeparamref name="TSort"/> that orders leases by a lease expression.
        /// </summary>
        /// <typeparam name="TLease">The type of lease in the lease expression.</typeparam>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <param name="leaseExpression">The lease expression.</param>
        /// <returns>A <typeparamref name="TSort"/> that oders leases by a lease expression.</returns>
        TSort LeaseProperty<TLease>(bool ascending, Expression<Func<TLease, object>> leaseExpression);
    }
}
