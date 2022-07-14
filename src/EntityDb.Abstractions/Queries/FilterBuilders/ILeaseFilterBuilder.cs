using EntityDb.Abstractions.ValueObjects;
using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for a lease query.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface ILeaseFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <ignore/>
    [Obsolete("This method will be removed in the future.")]
    TFilter LeaseMatches<TLease>(Expression<Func<TLease, bool>> leaseExpression)
        => throw new NotSupportedException();

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes leases with an entity id which is contained in a set
    ///     of entity ids.
    /// </summary>
    /// <param name="entityIds">The set of entity ids.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes leases with an entity id which is contained in
    ///     <paramref name="entityIds" />.
    /// </returns>
    TFilter EntityIdIn(params Id[] entityIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes leases with an entity version number greater than or
    ///     equal to an entity version number.
    /// </summary>
    /// <param name="entityVersionNumber">The entity version number.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes leases with an entity version number greater than or
    ///     equal to <paramref name="entityVersionNumber" />.
    /// </returns>
    TFilter EntityVersionNumberGte(VersionNumber entityVersionNumber);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes leases with an entity version number less than or
    ///     equal to an entity version number.
    /// </summary>
    /// <param name="entityVersionNumber">The entity version number.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes leases with an entity version number less than or equal
    ///     to <paramref name="entityVersionNumber" />.
    /// </returns>
    TFilter EntityVersionNumberLte(VersionNumber entityVersionNumber);

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
}
