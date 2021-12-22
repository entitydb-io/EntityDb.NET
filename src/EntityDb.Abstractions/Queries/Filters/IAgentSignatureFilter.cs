using EntityDb.Abstractions.Queries.FilterBuilders;

namespace EntityDb.Abstractions.Queries.Filters
{
    /// <summary>
    ///     Represents a type that supplies filtering for a <see cref="IAgentSignatureQuery" />.
    /// </summary>
    public interface IAgentSignatureFilter
    {
        /// <summary>
        ///     Returns a <typeparamref name="TFilter" /> built from a agentSignature filter builder.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
        /// <param name="builder">The agentSignature filter builder.</param>
        /// <returns>A <typeparamref name="TFilter" /> built from <paramref name="builder" />.</returns>
        TFilter GetFilter<TFilter>(IAgentSignatureFilterBuilder<TFilter> builder);
    }
}
