using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Sources.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for an object. Possible objects include:
///     agent signatures, deltas, facts, and tags.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IDataFilterBuilder<TFilter>
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with a source
    ///     timestamp greater than or equal to a source timestamp.
    /// </summary>
    /// <param name="sourceTimeStamp">The source timestamp.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes objects with an source timestamp
    ///     greater than or equal to <paramref name="sourceTimeStamp" />.
    /// </returns>
    TFilter SourceTimeStampGte(TimeStamp sourceTimeStamp);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with a source
    ///     timestamp less than or equal to a source timestamp.
    /// </summary>
    /// <param name="sourceTimeStamp">The source timestamp.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes objects with an source timestamp
    ///     less than or equal to <paramref name="sourceTimeStamp" />.
    /// </returns>
    TFilter SourceTimeStampLte(TimeStamp sourceTimeStamp);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects with an source
    ///     id which is contained in a set of source ids.
    /// </summary>
    /// <param name="sourceIds">The set of source ids.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes objects with an source id which
    ///     is contained in <paramref name="sourceIds" />.
    /// </returns>
    TFilter SourceIdIn(params Id[] sourceIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes objects whose data type
    ///     is contained in a set of data types.
    /// </summary>
    /// <param name="dataTypes">The data types.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes objects whose data type is contained
    ///     in <paramref name="dataTypes" />.
    /// </returns>
    TFilter DataTypeIn(params Type[] dataTypes);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do match a filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that excludes objects which do match
    ///     <paramref name="filter" />.
    /// </returns>
    TFilter Not(TFilter filter);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do not match
    ///     all filters in a set of filters.
    /// </summary>
    /// <param name="filters">The set of filters.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that excludes objects which do not match all filters in
    ///     <paramref name="filters" />.
    /// </returns>
    TFilter And(params TFilter[] filters);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that excludes objects which do not match
    ///     any filter in a set of filters.
    /// </summary>
    /// <param name="filters">The set of filters.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that excludes objects which do not match any filter
    ///     in <paramref name="filters" />.
    /// </returns>
    TFilter Or(params TFilter[] filters);
}
