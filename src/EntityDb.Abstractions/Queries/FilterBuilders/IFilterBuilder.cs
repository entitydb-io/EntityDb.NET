using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for an object repository. Possible objects include: agentSignatures,
///     commands, facts, tags, and events.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IFilterBuilder<TFilter>
{
    /// <ignore />
    [Obsolete("Please use SourceTimeStampGte instead. This will be removed in a future version.")]
    TFilter TransactionTimeStampGte(TimeStamp transactionTimeStamp) => SourceTimeStampGte(transactionTimeStamp);

    /// <ignore />
    [Obsolete("Please use SourceTimeStampLte instead. This will be removed in a future version.")]
    TFilter TransactionTimeStampLte(TimeStamp transactionTimeStamp) => SourceTimeStampLte(transactionTimeStamp);

    /// <ignore />
    [Obsolete("Please use SourceIdIn instead. This will be removed in a future version.")]
    TFilter TransactionIdIn(params Id[] transactionIds) => SourceIdIn(transactionIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with a source timestamp greater than or
    ///     equal to a source timestamp.
    /// </summary>
    /// <param name="sourceTimeStamp">The source timestamp.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes objects with an source timestamp greater than or
    ///     equal to <paramref name="sourceTimeStamp" />.
    /// </returns>
    TFilter SourceTimeStampGte(TimeStamp sourceTimeStamp);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with a source timestamp less than or
    ///     equal to a source timestamp.
    /// </summary>
    /// <param name="sourceTimeStamp">The source timestamp.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes objects with an source timestamp less than or equal
    ///     to <paramref name="sourceTimeStamp" />.
    /// </returns>
    TFilter SourceTimeStampLte(TimeStamp sourceTimeStamp);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an source id which is contained in a
    ///     set of source ids.
    /// </summary>
    /// <param name="sourceIds">The set of source ids.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes objects with an source id which is contained in
    ///     <paramref name="sourceIds" />.
    /// </returns>
    TFilter SourceIdIn(params Id[] sourceIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do match a filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>A <typeparamref name="TFilter" /> that excludes objects which do match <paramref name="filter" />.</returns>
    TFilter Not(TFilter filter);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do not match all filters in a set of filters.
    /// </summary>
    /// <param name="filters">The set of filters.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that excludes objects which do not match all filters in
    ///     <paramref name="filters" />.
    /// </returns>
    TFilter And(params TFilter[] filters);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do not match any filter in a set of filters.
    /// </summary>
    /// <param name="filters">The set of filters.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that excludes objects which do not match any filter in
    ///     <paramref name="filters" />.
    /// </returns>
    TFilter Or(params TFilter[] filters);
}
