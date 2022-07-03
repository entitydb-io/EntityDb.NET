using EntityDb.Abstractions.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Transactions.Builders;

/// <summary>
///     Represents a type used to create instances of <see cref="ITransactionBuilder{TEntity}" /> or
///     <see cref="ISingleEntityTransactionBuilder{TEntity}" />.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ITransactionBuilderFactory<TEntity>
{
    /// <summary>
    ///     Creates a new instance of <see cref="ITransactionBuilder{TEntity}" />.
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ITransactionBuilder{TEntity}" />.</returns>
    Task<ITransactionBuilder<TEntity>> Create(string agentSignatureOptionsName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new instance of <see cref="ISingleEntityTransactionBuilder{TEntity}" />.
    /// </summary>
    /// <param name="agentSignatureOptionsName">The name of the agent signature options.</param>
    /// <param name="entityId">The id of the entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ITransactionBuilder{TEntity}" />.</returns>
    Task<ISingleEntityTransactionBuilder<TEntity>> CreateForSingleEntity(string agentSignatureOptionsName, Id entityId,
        CancellationToken cancellationToken = default);
}
