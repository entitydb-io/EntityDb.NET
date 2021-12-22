using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Queries.SortBuilders;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    ///     Extensions for sort builders.
    /// </summary>
    public static class ISortBuilderExtensions
    {
        /// <summary>
        ///     Returns a <see cref="IAgentSignatureSortBuilder{TSort}" /> that orders agentSignatures in the reverse order of another
        ///     <see cref="IAgentSignatureSortBuilder{TSort}" />.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="agentSignatureSortBuilder">The agentSignature sort builder.</param>
        /// <returns>
        ///     A <see cref="IAgentSignatureSortBuilder{TSort}" /> that orders agentSignatures in the reverse order of
        ///     <paramref name="agentSignatureSortBuilder" />.
        /// </returns>
        public static IAgentSignatureSortBuilder<TSort> Reverse<TSort>(this IAgentSignatureSortBuilder<TSort> agentSignatureSortBuilder)
        {
            return new AgentSignatureReverseSortBuilder<TSort>(agentSignatureSortBuilder);
        }

        /// <summary>
        ///     Returns a <see cref="ICommandSortBuilder{TSort}" /> that orders commands in the reverse order of another
        ///     <see cref="ICommandSortBuilder{TSort}" />.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="commandSortBuilder">The command sort builder.</param>
        /// <returns>
        ///     A <see cref="ICommandSortBuilder{TSort}" /> that orders commands in the reverse order of
        ///     <paramref name="commandSortBuilder" />.
        /// </returns>
        public static ICommandSortBuilder<TSort> Reverse<TSort>(this ICommandSortBuilder<TSort> commandSortBuilder)
        {
            return new CommandReverseSortBuilder<TSort>(commandSortBuilder);
        }

        /// <summary>
        ///     Returns a <see cref="ILeaseSortBuilder{TSort}" /> that orders leases in the reverse order of another
        ///     <see cref="ILeaseSortBuilder{TSort}" />.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="leaseSortBuilder">The lease sort builder.</param>
        /// <returns>
        ///     A <see cref="ILeaseSortBuilder{TSort}" /> that orders leases in the reverse order of
        ///     <paramref name="leaseSortBuilder" />.
        /// </returns>
        public static ILeaseSortBuilder<TSort> Reverse<TSort>(this ILeaseSortBuilder<TSort> leaseSortBuilder)
        {
            return new LeaseReverseSortBuilder<TSort>(leaseSortBuilder);
        }

        /// <summary>
        ///     Returns a <see cref="ITagSortBuilder{TSort}" /> that orders tags in the reverse order of another
        ///     <see cref="ITagSortBuilder{TSort}" />.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="tagSortBuilder">The tag sort builder.</param>
        /// <returns>
        ///     A <see cref="ITagSortBuilder{TSort}" /> that orders tags in the reverse order of
        ///     <paramref name="tagSortBuilder" />.
        /// </returns>
        public static ITagSortBuilder<TSort> Reverse<TSort>(this ITagSortBuilder<TSort> tagSortBuilder)
        {
            return new TagReverseSortBuilder<TSort>(tagSortBuilder);
        }
    }
}
