using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.Sources.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for a lease query.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface ILeaseDataFilterBuilder<TFilter> : IMessageDataFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes leases whose <see cref="ILease.Scope" /> is
    ///     a particular value.
    /// </summary>
    /// <param name="scope">The lease scope</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes leases whose <see cref="ILease.Scope" /> is
    ///     <paramref name="scope" />.
    /// </returns>
    TFilter LeaseScopeEq(string scope);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes leases whose <see cref="ILease.Label" /> is
    ///     a particular value.
    /// </summary>
    /// <param name="label">The lease label</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes leases whose <see cref="ILease.Label" /> is
    ///     <paramref name="label" />.
    /// </returns>
    TFilter LeaseLabelEq(string label);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes leases whose <see cref="ILease.Value" /> is
    ///     a particular value.
    /// </summary>
    /// <param name="value">The lease value</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes leases whose <see cref="ILease.Value" /> is
    ///     <paramref name="value" />.
    /// </returns>
    TFilter LeaseValueEq(string value);
}
