using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Common.Sources.Queries.Modified;

namespace EntityDb.Common.Sources.Queries;

/// <summary>
///     Extensions for queries.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    ///     Returns a new, modified <see cref="IMessageGroupQuery" />. The way in which
    ///     it is modified depends on the parameters of this extension method.
    /// </summary>
    /// <param name="messageGroupQuery">The message group query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="IMessageGroupQuery" />.</returns>
    public static IMessageGroupQuery Modify(this IMessageGroupQuery messageGroupQuery,
        ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedMessageGroupQuery(messageGroupQuery, modifiedQueryOptions);
    }

    /// <summary>
    ///     Returns a new, modified <see cref="IMessageQuery" />. The way in which it is modified depends on the parameters of
    ///     this extension method.
    /// </summary>
    /// <param name="messageQuery">The message query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="IMessageQuery" />.</returns>
    public static IMessageQuery Modify(this IMessageQuery messageQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedMessageQuery(messageQuery, modifiedQueryOptions);
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
        return new ModifiedLeaseQuery(leaseQuery, modifiedQueryOptions);
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
        return new ModifiedTagQuery(tagQuery, modifiedQueryOptions);
    }
}
