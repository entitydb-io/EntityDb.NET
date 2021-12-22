using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.Filters;
using EntityDb.Common.Queries.Filtered;
using EntityDb.Common.Queries.Modified;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    ///     Extensions for queries.
    /// </summary>
    public static class IQueryExtensions
    {
        /// <summary>
        ///     Returns a new <see cref="IAgentSignatureQuery" /> that "ands" a agentSignature filter with the filter of a agentSignature query. All other
        ///     properties of the query are inherited.
        /// </summary>
        /// <param name="agentSignatureQuery">The agentSignature query.</param>
        /// <param name="agentSignatureFilter">The agentSignature filter.</param>
        /// <returns>
        ///     A new <see cref="IAgentSignatureQuery" /> that "ands" <paramref name="agentSignatureFilter" /> with the filter of
        ///     <paramref name="agentSignatureQuery" />.
        /// </returns>
        public static IAgentSignatureQuery Filter(this IAgentSignatureQuery agentSignatureQuery, IAgentSignatureFilter agentSignatureFilter)
        {
            return new FilteredAgentSignatureQuery(agentSignatureQuery, agentSignatureFilter);
        }

        /// <summary>
        ///     Returns a new <see cref="ICommandQuery" /> that "ands" a command filter with the filter of a command query. All
        ///     other properties of the query are inherited.
        /// </summary>
        /// <param name="commandQuery">The command query.</param>
        /// <param name="commandFilter">The command filter.</param>
        /// <returns>
        ///     A new <see cref="ICommandQuery" /> that "ands" <paramref name="commandFilter" /> with the filter of
        ///     <paramref name="commandQuery" />.
        /// </returns>
        public static ICommandQuery Filter(this ICommandQuery commandQuery, ICommandFilter commandFilter)
        {
            return new FilteredCommandQuery(commandQuery, commandFilter);
        }

        /// <summary>
        ///     Returns a new <see cref="ILeaseQuery" /> that "ands" a lease filter with the filter of a lease query. All other
        ///     properties of the query are inherited.
        /// </summary>
        /// <param name="leaseQuery">The lease query.</param>
        /// <param name="leaseFilter">The lease filter.</param>
        /// <returns>
        ///     A new <see cref="ILeaseQuery" /> that "ands" <paramref name="leaseFilter" /> with the filter of
        ///     <paramref name="leaseQuery" />.
        /// </returns>
        public static ILeaseQuery Filter(this ILeaseQuery leaseQuery, ILeaseFilter leaseFilter)
        {
            return new FilteredLeaseQuery(leaseQuery, leaseFilter);
        }

        /// <summary>
        ///     Returns a new <see cref="ITagQuery" /> that "ands" a tag filter with the filter of a tag query. All other
        ///     properties of the query are inherited.
        /// </summary>
        /// <param name="tagQuery">The tag query.</param>
        /// <param name="tagFilter">The tag filter.</param>
        /// <returns>
        ///     A new <see cref="ITagQuery" /> that "ands" <paramref name="tagFilter" /> with the filter of
        ///     <paramref name="tagQuery" />.
        /// </returns>
        public static ITagQuery Filter(this ITagQuery tagQuery, ITagFilter tagFilter)
        {
            return new FilteredTagQuery(tagQuery, tagFilter);
        }

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
}
