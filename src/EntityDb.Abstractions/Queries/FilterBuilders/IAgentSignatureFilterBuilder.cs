using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Queries.FilterBuilders;

/// <summary>
///     Builds a <typeparamref name="TFilter" /> for a agentSignature query.
/// </summary>
/// <typeparam name="TFilter">The type of filter used by the repository.</typeparam>
public interface IAgentSignatureFilterBuilder<TFilter> : IFilterBuilder<TFilter>
{
    /// <ignore />
    [Obsolete("Please use SubjectIdsIn instead. This will be removed in a future version.")]
    TFilter EntityIdsIn(params Id[] entityIds) => SubjectIdsIn(entityIds);

    /// <summary>
    ///     Returns a <typeparamref name="TFilter" /> that only includes agentSignatures with any source id which is contained
    ///     in a set of source ids.
    /// </summary>
    /// <param name="subjectIds">The set of subject ids.</param>
    /// <returns>
    ///     A <typeparamref name="TFilter" /> that only includes agentSignatures with any source id which is contained in
    ///     <paramref name="subjectIds" />.
    /// </returns>
    TFilter SubjectIdsIn(params Id[] subjectIds);

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
