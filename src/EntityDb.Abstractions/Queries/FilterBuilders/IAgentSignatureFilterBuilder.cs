using EntityDb.Abstractions.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for a agentSignature query.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IAgentSignatureFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <ignore/>
    [Obsolete("This method will be removed in the future, and may not be supported for all implementations.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    TFilter AgentSignatureMatches<TAgentSignature>(Expression<Func<TAgentSignature, bool>> agentSignatureExpression)
        => throw new NotSupportedException();

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes agentSignatures with any entity id which is contained
    ///     in a set
    ///     of entity ids.
    /// </summary>
    /// <param name="entityIds">The set of entity ids.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes agentSignatures with any entity id which is contained in
    ///     <paramref name="entityIds" />.
    /// </returns>
    TFilter EntityIdsIn(params Id[] entityIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes agentSignatures whose type is contained in a set of
    ///     agentSignature
    ///     types.
    /// </summary>
    /// <param name="agentSignatureTypes">The agentSignature types.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes agentSignatures whose type is contained in
    ///     <paramref name="agentSignatureTypes" />.
    /// </returns>
    TFilter AgentSignatureTypeIn(params Type[] agentSignatureTypes);
}
