using EntityDb.Abstractions.Leases;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.SortBuilders;

/// <summary>
///     Builds a sort for a lease query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface ILeaseSortBuilder<TSort> : ISortBuilder<TSort>
{
    /// <ignore/>
    [Obsolete("This method will be removed in the future, and may not be supported for all implementations.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    TSort LeaseProperty<TLease>(bool ascending, Expression<Func<TLease, object>> leaseExpression)
        => throw new NotSupportedException();

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by entity id.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by entity id.</returns>
    TSort EntityId(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by entity version number.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by entity version number.</returns>
    TSort EntityVersionNumber(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by type.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by type.</returns>
    TSort LeaseType(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Scope"/>.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Scope"/>.</returns>
    TSort LeaseScope(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Label"/>.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Label"/>.</returns>
    TSort LeaseLabel(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Value"/>.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Value"/>.</returns>
    TSort LeaseValue(bool ascending);
}
