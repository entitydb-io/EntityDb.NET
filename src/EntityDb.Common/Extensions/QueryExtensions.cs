using EntityDb.Abstractions.Queries;
using EntityDb.Common.Queries.Modified;

namespace EntityDb.Common.Extensions;

/// <summary>
///     Extensions for queries.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    ///     Returns a new, modified <see cref="IAgentSignatureQuery" />. The way in which it is modified depends on the parameters of
    ///     this extension method.
    /// </summary>
    /// <param name="agentSignatureQuery">The agentSignature query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="IAgentSignatureQuery" />.</returns>
    public static IAgentSignatureQuery Modify(this IAgentSignatureQuery agentSignatureQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedAgentSignatureQuery(agentSignatureQuery, modifiedQueryOptions);
    }

    /// <summary>
    ///     Returns a new, modified <see cref="ICommandQuery" />. The way in which it is modified depends on the parameters of
    ///     this extension method.
    /// </summary>
    /// <param name="commandQuery">The command query.</param>
    /// <param name="modifiedQueryOptions">The options for modifying the query.</param>
    /// <returns>A new, modified <see cref="ICommandQuery" />.</returns>
    public static ICommandQuery Modify(this ICommandQuery commandQuery, ModifiedQueryOptions modifiedQueryOptions)
    {
        return new ModifiedCommandQuery(commandQuery, modifiedQueryOptions);
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
