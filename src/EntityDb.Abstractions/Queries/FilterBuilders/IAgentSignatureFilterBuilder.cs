using EntityDb.Abstractions.ValueObjects;
using System;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for a agentSignature query.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IAgentSignatureFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
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

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes agentSignatures which do match a agentSignature
    ///     expression.
    /// </summary>
    /// <typeparam name="TAgentSignature">The type of agentSignature in the agentSignature expression.</typeparam>
    /// <param name="agentSignatureExpression">The agentSignature expression.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes agentSignatures which do match
    ///     <paramref name="agentSignatureExpression" />.
    /// </returns>
    TFilter AgentSignatureMatches<TAgentSignature>(Expression<Func<TAgentSignature, bool>> agentSignatureExpression);
}
