namespace EntityDb.Abstractions.Queries.SortBuilders;

/// <summary>
///     Builds a <typeparamref name="TSort" /> for a agentSignature query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface IAgentSignatureSortBuilder<TSort> : ISortBuilder<TSort>
{
    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders agentSignatures by entity ids.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders agentSignatures by entity ids.</returns>
    TSort EntityIds(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders agentSignatures by type.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders agentSignatures by type.</returns>
    TSort AgentSignatureType(bool ascending);
}
