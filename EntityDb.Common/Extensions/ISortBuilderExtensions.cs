using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Common.Queries.SortBuilders;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    /// Extensions for sort builders.
    /// </summary>
    public static class ISortBuilderExtensions
    {
        /// <summary>
        /// Returns a <see cref="ISourceSortBuilder{TSort}"/> that orders sources in the reverse order of another <see cref="ISourceSortBuilder{TSort}"/>.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="sourceSortBuilder">The source sort builder.</param>
        /// <returns>A <see cref="ISourceSortBuilder{TSort}"/> that orders sources in the reverse order of <paramref name="sourceSortBuilder"/>.</returns>
        public static ISourceSortBuilder<TSort> Reverse<TSort>(this ISourceSortBuilder<TSort> sourceSortBuilder)
        {
            return new SourceReverseSortBuilder<TSort>(sourceSortBuilder);
        }

        /// <summary>
        /// Returns a <see cref="ICommandSortBuilder{TSort}"/> that orders commands in the reverse order of another <see cref="ICommandSortBuilder{TSort}"/>.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="commandSortBuilder">The command sort builder.</param>
        /// <returns>A <see cref="ICommandSortBuilder{TSort}"/> that orders commands in the reverse order of <paramref name="commandSortBuilder"/>.</returns>
        public static ICommandSortBuilder<TSort> Reverse<TSort>(this ICommandSortBuilder<TSort> commandSortBuilder)
        {
            return new CommandReverseSortBuilder<TSort>(commandSortBuilder);
        }

        /// <summary>
        /// Returns a <see cref="IFactSortBuilder{TSort}"/> that orders facts in the reverse order of another <see cref="IFactSortBuilder{TSort}"/>.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="factSortBuilder">The fact sort builder.</param>
        /// <returns>A <see cref="IFactSortBuilder{TSort}"/> that orders facts in the reverse order of <paramref name="factSortBuilder"/>.</returns>
        public static IFactSortBuilder<TSort> Reverse<TSort>(this IFactSortBuilder<TSort> factSortBuilder)
        {
            return new FactReverseSortBuilder<TSort>(factSortBuilder);
        }

        /// <summary>
        /// Returns a <see cref="ILeaseSortBuilder{TSort}"/> that orders leases in the reverse order of another <see cref="ILeaseSortBuilder{TSort}"/>.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="leaseSortBuilder">The lease sort builder.</param>
        /// <returns>A <see cref="ILeaseSortBuilder{TSort}"/> that orders leases in the reverse order of <paramref name="leaseSortBuilder"/>.</returns>
        public static ILeaseSortBuilder<TSort> Reverse<TSort>(this ILeaseSortBuilder<TSort> leaseSortBuilder)
        {
            return new LeaseReverseSortBuilder<TSort>(leaseSortBuilder);
        }
    }
}
