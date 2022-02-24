using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;

namespace EntityDb.Abstractions.Queries;

/// <summary>
///     Abstracts a query on agentSignatures.
/// </summary>
public interface IAgentSignatureQuery : IQuery
{
    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> built from a agentSignature filter builder.
    /// </summary>
    /// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
    /// <param name="builder">The agentSignature filter builder.</param>
    /// <returns>A <typeparamref name="TFilter" /> built from <paramref name="builder" />.</returns>
    TFilter GetFilter<TFilter>(IAgentSignatureFilterBuilder<TFilter> builder);
    
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> built from a agentSignature sort builder.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The agentSignature sort builder.</param>
    /// <returns>A <typeparamref name="TSort" /> built from <paramref name="builder" />.</returns>
    TSort? GetSort<TSort>(IAgentSignatureSortBuilder<TSort> builder);
}
