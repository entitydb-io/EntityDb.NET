using System;

namespace EntityDb.Abstractions.Queries.FilterBuilders
{
    /// <summary>
    /// Builds a <typeparamref name="TFilter"/> for an object repository. Possible objects include: sources, commands, facts, and leases.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    public interface IFilterBuilder<TFilter>
    {
        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes objects with a transaction timestamp greater than or equal to a transaction timestamp.
        /// </summary>
        /// <param name="transactionTimeStamp">The transaction timestamp.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes objects with an transaction timestamp greater than or equal to <paramref name="transactionTimeStamp"/>.</returns>
        TFilter TransactionTimeStampGte(DateTime transactionTimeStamp);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes objects with a transaction timestamp less than or equal to a transaction timestamp.
        /// </summary>
        /// <param name="transactionTimeStamp">The transaction timestamp.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes objects with an transaction timestamp less than or equal to <paramref name="transactionTimeStamp"/>.</returns>
        TFilter TransactionTimeStampLte(DateTime transactionTimeStamp);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that only includes objects with an transaction id which is contained in a set of transaction ids.
        /// </summary>
        /// <param name="transactionIds">The set of transaction ids.</param>
        /// <returns>A <typeparamref name="TFilter"/> that only includes objects with an transaction id which is contained in <paramref name="transactionIds"/>.</returns>
        TFilter TransactionIdIn(params Guid[] transactionIds);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that excludes objects which do match a filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>A <typeparamref name="TFilter"/> that excludes objects which do match <paramref name="filter"/>.</returns>
        TFilter Not(TFilter filter);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that excludes objects which do not match all filters in a set of filters.
        /// </summary>
        /// <param name="filters">The set of filters.</param>
        /// <returns>A <typeparamref name="TFilter"/> that excludes objects which do not match all filters in <paramref name="filters"/>.</returns>
        TFilter And(params TFilter[] filters);

        /// <summary>
        /// Returns a <typeparamref name="TFilter"/> that excludes objects which do not match any filter in a set of filters.
        /// </summary>
        /// <param name="filters">The set of filters.</param>
        /// <returns>A <typeparamref name="TFilter"/> that excludes objects which do not match any filter in <paramref name="filters"/>.</returns>
        TFilter Or(params TFilter[] filters);
    }
}
