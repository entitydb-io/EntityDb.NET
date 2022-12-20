using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Queries;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace EntityDb.Common.Projections;

internal sealed class ProjectionRepository<TProjection> : DisposableResourceBaseClass,
    IProjectionRepository<TProjection>
    where TProjection : IProjection<TProjection>
{
    public ProjectionRepository
    (
        ITransactionRepository transactionRepository,
        ISnapshotRepository<TProjection>? snapshotRepository = null
    )
    {
        TransactionRepository = transactionRepository;
        SnapshotRepository = snapshotRepository;
    }

    public ITransactionRepository TransactionRepository { get; }

    public ISnapshotRepository<TProjection>? SnapshotRepository { get; }

    public Id? GetProjectionIdOrDefault(ITransaction transaction, ITransactionCommand transactionCommand)
    {
        return TProjection.GetProjectionIdOrDefault(transaction, transactionCommand);
    }

    public async Task<TProjection> GetSnapshot(Pointer projectionPointer, CancellationToken cancellationToken = default)
    {
        var projection = SnapshotRepository is not null
            ? await SnapshotRepository.GetSnapshotOrDefault(projectionPointer, cancellationToken) ??
              TProjection.Construct(projectionPointer.Id)
            : TProjection.Construct(projectionPointer.Id);

        var newTransactionIdsQuery = await projection.GetCommandQuery(projectionPointer, TransactionRepository, cancellationToken);

        var transactions = EnumerateTransactions(newTransactionIdsQuery, cancellationToken);

        await foreach (var transaction in transactions)
        {
            foreach (var transactionCommand in transaction.Commands)
            {
                projection = projection.Reduce(transaction, transactionCommand);
            }
        }

        if (!projectionPointer.IsSatisfiedBy(projection.GetVersionNumber()))
        {
            throw new SnapshotPointerDoesNotExistException();
        }

        return projection;
    }

    public static IProjectionRepository<TProjection> Create(IServiceProvider serviceProvider,
        ITransactionRepository transactionRepository,
        ISnapshotRepository<TProjection>? snapshotRepository = null)
    {
        if (snapshotRepository is null)
        {
            return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
                transactionRepository);
        }

        return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
            transactionRepository, snapshotRepository);
    }

    private async IAsyncEnumerable<ITransaction> EnumerateTransactions(ICommandQuery commandQuery, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var allAnnotatedCommands = await TransactionRepository
            .EnumerateAnnotatedCommands(commandQuery, cancellationToken)
            .ToLookupAsync(annotatedCommand => annotatedCommand.TransactionId, cancellationToken);

        var transactionIds = allAnnotatedCommands
            .SelectMany(annotatedCommands => annotatedCommands
                .Select(annotatedCommand => annotatedCommand.TransactionId))
            .Distinct()
            .ToArray();

        var agentSignatureQuery = new GetAgentSignatures(transactionIds);

        var annotatedAgentSignatures = TransactionRepository
            .EnumerateAnnotatedAgentSignatures(agentSignatureQuery, cancellationToken);

        await foreach (var annotatedAgentSignature in annotatedAgentSignatures)
        {
            var annotatedCommands = allAnnotatedCommands[annotatedAgentSignature.TransactionId];

            yield return new Transaction
            {
                Id = annotatedAgentSignature.TransactionId,
                TimeStamp = annotatedAgentSignature.TransactionTimeStamp,
                AgentSignature = annotatedAgentSignature.Data,
                Commands = annotatedCommands
                    .Select(annotatedCommand => new TransactionCommand
                    {
                        EntityId = annotatedCommand.EntityId,
                        EntityVersionNumber = annotatedCommand.EntityVersionNumber,
                        Command = annotatedCommand.Data
                    })
                    .ToImmutableArray<ITransactionCommand>()
            };
        }
    }
}
