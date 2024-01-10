using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Common.Sources.Queries.Modified;

namespace EntityDb.Common.Sources.Queries;

/// <summary>
///     Extensions for queries.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    ///     Returns a new, modified <see cref="ISourceDataDataQuery" />. The way in which
    ///     it is modified depends on the parameters of this extension method.
    /// </summary>
    /// <param name="sourceDataDataQuery">The source data query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ISourceDataDataQuery" />.</returns>
    public static ISourceDataDataQuery Modify(this ISourceDataDataQuery sourceDataDataQuery,
        ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedSourceDataDataQuery
        {
            ModifiedQueryOptions = modifiedQueryOptions, SourceDataDataQuery = sourceDataDataQuery,
        };
    }

    /// <summary>
    ///     Returns a new, modified <see cref="IMessageDataDataQuery" />. The way in which it is modified depends on the parameters
    ///     of
    ///     this extension method.
    /// </summary>
    /// <param name="messageDataDataQuery">The message data query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="IMessageDataDataQuery" />.</returns>
    public static IMessageDataDataQuery Modify(this IMessageDataDataQuery messageDataDataQuery,
        ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedMessageDataDataQuery
        {
            ModifiedQueryOptions = modifiedQueryOptions, MessageDataDataQuery = messageDataDataQuery,
        };
    }

    /// <summary>
    ///     Returns a new, modified <see cref="ILeaseDataDataQuery" />. The way in which it is modified depends on the parameters of
    ///     this extension method.
    /// </summary>
    /// <param name="leaseDataDataQuery">The lease query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ILeaseDataDataQuery" />.</returns>
    public static ILeaseDataDataQuery Modify(this ILeaseDataDataQuery leaseDataDataQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedLeaseDataDataQuery { ModifiedQueryOptions = modifiedQueryOptions, LeaseDataDataQuery = leaseDataDataQuery };
    }

    /// <summary>
    ///     Returns a new, modified <see cref="ITagDataDataQuery" />. The way in which it is modified depends on the parameters of this
    ///     extension method.
    /// </summary>
    /// <param name="tagDataDataQuery">The tag query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ITagDataDataQuery" />.</returns>
    public static ITagDataDataQuery Modify(this ITagDataDataQuery tagDataDataQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedTagDataDataQuery { ModifiedQueryOptions = modifiedQueryOptions, TagDataDataQuery = tagDataDataQuery };
    }
}
