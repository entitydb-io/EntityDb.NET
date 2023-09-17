using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Queries;
using EntityDb.Common.Transactions;
using System.Collections.Immutable;

namespace EntityDb.Common.Extensions;

/// <summary>
///     Extensions for <see cref="ITransactionRepository"/>.
/// </summary>
public static class TransactionRepositoryExtensions
{
    /// <summary>
    ///     Enumerate transaction ids for any supported query type
    /// </summary>
    /// <param name="transactionRepository">The transaction repository</param>
    /// <param name="query">The query</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Transaction ids</returns>
    public static IAsyncEnumerable<Id> EnumerateTransactionIds(this ITransactionRepository transactionRepository, IQuery query, CancellationToken cancellationToken = default)
    {
        return query switch
        {
            IAgentSignatureQuery agentSignatureQuery => transactionRepository.EnumerateTransactionIds(agentSignatureQuery, cancellationToken),
            ICommandQuery commandQuery => transactionRepository.EnumerateTransactionIds(commandQuery, cancellationToken),
            ILeaseQuery leaseQuery => transactionRepository.EnumerateTransactionIds(leaseQuery, cancellationToken),
            ITagQuery tagQuery => transactionRepository.EnumerateTransactionIds(tagQuery, cancellationToken),
            _ => AsyncEnumerable.Empty<Id>(),
        };
    }

    /// <summary>
    ///     Reconstruct the <see cref="ITransaction"/> object by transaction id
    /// </summary>
    /// <param name="transactionRepository">The transaction repository</param>
    /// <param name="transactionId">The transaction id</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>An instance of <see cref="ITransaction"/>.</returns>
    public static async Task<ITransaction> GetTransaction(this ITransactionRepository transactionRepository, Id transactionId, CancellationToken cancellationToken)
    {
        var query = new GetTransactionCommandsQuery(transactionId);

        var annotatedAgentSignature = await transactionRepository
            .EnumerateAnnotatedAgentSignatures(query, cancellationToken)
            .SingleAsync(cancellationToken);

        var annotatedCommands = await transactionRepository
            .EnumerateAnnotatedCommands(query, cancellationToken)
            .Select(annotatedCommand => new TransactionCommand
            {
                EntityId = annotatedCommand.EntityId,
                EntityVersionNumber = annotatedCommand.EntityVersionNumber,
                Data = annotatedCommand.Data
            })
            .ToArrayAsync(cancellationToken);

        return new Transaction
        {
            Id = annotatedAgentSignature.TransactionId,
            TimeStamp = annotatedAgentSignature.TransactionTimeStamp,
            AgentSignature = annotatedAgentSignature.Data,
            Commands = annotatedCommands.ToImmutableArray<ITransactionCommand>(),
        };
    }
}
