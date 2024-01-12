using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Common.Sources.Queries.Modified;

namespace EntityDb.Common.Sources.Queries;

/// <summary>
///     Extensions for queries.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    ///     Returns a new, modified <see cref="ISourceDataQuery" />. The way in which
    ///     it is modified depends on the parameters of this extension method.
    /// </summary>
    /// <param name="sourceDataQuery">The source data query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ISourceDataQuery" />.</returns>
    public static ISourceDataQuery Modify(this ISourceDataQuery sourceDataQuery,
        ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedSourceDataQuery
        {
            ModifiedQueryOptions = modifiedQueryOptions, SourceDataQuery = sourceDataQuery,
        };
    }

    /// <summary>
    ///     Returns a new, modified <see cref="IMessageDataQuery" />. The way in which it is modified depends on the parameters
    ///     of
    ///     this extension method.
    /// </summary>
    /// <param name="messageDataQuery">The message data query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="IMessageDataQuery" />.</returns>
    public static IMessageDataQuery Modify(this IMessageDataQuery messageDataQuery,
        ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedMessageDataQuery
        {
            ModifiedQueryOptions = modifiedQueryOptions, MessageDataQuery = messageDataQuery,
        };
    }

    /// <summary>
    ///     Returns a new, modified <see cref="ILeaseDataQuery" />. The way in which it is modified depends on the parameters of
    ///     this extension method.
    /// </summary>
    /// <param name="leaseDataQuery">The lease query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ILeaseDataQuery" />.</returns>
    public static ILeaseDataQuery Modify(this ILeaseDataQuery leaseDataQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedLeaseDataQuery { ModifiedQueryOptions = modifiedQueryOptions, LeaseDataQuery = leaseDataQuery };
    }

    /// <summary>
    ///     Returns a new, modified <see cref="ITagDataQuery" />. The way in which it is modified depends on the parameters of this
    ///     extension method.
    /// </summary>
    /// <param name="tagDataQuery">The tag query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ITagDataQuery" />.</returns>
    public static ITagDataQuery Modify(this ITagDataQuery tagDataQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedTagDataQuery { ModifiedQueryOptions = modifiedQueryOptions, TagDataQuery = tagDataQuery };
    }
}
