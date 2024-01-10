using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Abstractions.Sources.Queries.SortBuilders;

/// <summary>
///     Builds a sort for a lease query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface ILeaseSortBuilder<TSort> : IMessageDataSortBuilder<TSort>
{
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Scope" />.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Scope" />.</returns>
    TSort LeaseScope(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Label" />.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Label" />.</returns>
    TSort LeaseLabel(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Value" />.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders leases by <see cref="ILease.Value" />.</returns>
    TSort LeaseValue(bool ascending);
}
