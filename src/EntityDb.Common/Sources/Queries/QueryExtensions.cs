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
    ///     Returns a new, modified <see cref="ILeaseQuery" />. The way in which it is modified depends on the parameters of
    ///     this extension method.
    /// </summary>
    /// <param name="leaseQuery">The lease query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ILeaseQuery" />.</returns>
    public static ILeaseQuery Modify(this ILeaseQuery leaseQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedLeaseQuery { ModifiedQueryOptions = modifiedQueryOptions, LeaseQuery = leaseQuery };
    }

    /// <summary>
    ///     Returns a new, modified <see cref="ITagQuery" />. The way in which it is modified depends on the parameters of this
    ///     extension method.
    /// </summary>
    /// <param name="tagQuery">The tag query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ITagQuery" />.</returns>
    public static ITagQuery Modify(this ITagQuery tagQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedTagQuery { ModifiedQueryOptions = modifiedQueryOptions, TagQuery = tagQuery };
    }
}
