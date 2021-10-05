using EntityDb.Abstractions.Queries.FilterBuilders;

namespace EntityDb.Abstractions.Queries.Filters
{
    /// <summary>
    ///     Represents a type that supplies filtering for a <see cref="IFactQuery" />.
    /// </summary>
    public interface IFactFilter
    {
        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> built from a fact filter builder.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="builder">The fact filter builder.</param>
        /// <returns>A <typeparamref name="TFilter" /> built from <paramref name="builder" />.</returns>
        TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder);
    }
}
