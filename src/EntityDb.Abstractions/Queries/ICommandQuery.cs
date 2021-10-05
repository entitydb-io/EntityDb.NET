using EntityDb.Abstractions.Queries.Filters;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries
{
    /// <summary>
    ///     Abstracts a query on commands.
    /// </summary>
    public interface ICommandQuery : IQuery, ICommandFilter
    {
        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> built from a command sort builder.
        /// </summary>
        /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
        /// <param name="builder">The command sort builder.</param>
        /// <returns>A <typeparamref name="TSort" /> built from <paramref name="builder" />.</returns>
        TSort? GetSort<TSort>(ICommandSortBuilder<TSort> builder);
    }
}
