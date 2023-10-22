using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Transactions.Sessions;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Transactions;

internal class MongoDbTransactionRepository : DisposableResourceBaseClass, ITransactionRepository
{
    private readonly IEnvelopeService<BsonDocument> _envelopeService;
    private readonly IMongoSession _mongoSession;

    public MongoDbTransactionRepository
    (
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService
    )
    {
        _mongoSession = mongoSession;
        _envelopeService = envelopeService;
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .EnumerateTransactionIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .EnumerateTransactionIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateTransactionIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateTransactionIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .EnumerateEntitiesIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .EnumerateEntityIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateEntityIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateEntityIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .EnumerateData<AgentSignatureDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateCommands(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .EnumerateData<CommandDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateData<LeaseDocument, ILease>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateData<TagDocument, ITag>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<IEntitiesAnnotation<object>> EnumerateAnnotatedAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .EnumerateEntitiesAnnotation<AgentSignatureDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<IEntityAnnotation<object>> EnumerateAnnotatedCommands(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .EnumerateEntityAnnotation<CommandDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public async Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            _mongoSession.StartTransaction();

            await PutAgentSignature(transaction, cancellationToken);

            foreach (var transactionCommand in transaction.Commands)
            {
                cancellationToken.ThrowIfCancellationRequested();

                VersionZeroReservedException.ThrowIfZero(transactionCommand.EntityVersionNumber);

                var previousVersionNumber = await CommandDocument
                    .GetLastEntityVersionNumber(_mongoSession, transactionCommand.EntityId, cancellationToken);

                OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber.Next(),
                    transactionCommand.EntityVersionNumber);

                await PutCommand(transaction, transactionCommand, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _mongoSession.CommitTransaction(cancellationToken);

            return true;
        }
        catch
        {
            await _mongoSession.AbortTransaction();

            throw;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoSession.DisposeAsync();
    }

    private async Task PutAgentSignature(ITransaction transaction, CancellationToken cancellationToken)
    {
        await AgentSignatureDocument
            .GetInsertCommand(_envelopeService, transaction)
            .Execute(_mongoSession, cancellationToken);
    }

    private async Task PutCommand(ITransaction transaction, ITransactionCommand transactionCommand,
        CancellationToken cancellationToken)
    {
        await CommandDocument
            .GetInsertCommand(_envelopeService, transaction, transactionCommand)
            .Execute(_mongoSession, cancellationToken);

        if (transactionCommand.Data is IAddLeasesCommand addLeasesCommand)
        {
            await LeaseDocument
                .GetInsertCommand(_envelopeService, transaction, transactionCommand, addLeasesCommand)
                .Execute(_mongoSession, cancellationToken);
        }

        if (transactionCommand.Data is IAddTagsCommand addTagsCommand)
        {
            await TagDocument
                .GetInsertCommand(_envelopeService, transaction, transactionCommand, addTagsCommand)
                .Execute(_mongoSession, cancellationToken);
        }

        if (transactionCommand.Data is IDeleteLeasesCommand deleteLeasesCommand)
        {
            await LeaseDocument
                .GetDeleteCommand(transactionCommand, deleteLeasesCommand)
                .Execute(_mongoSession, cancellationToken);
        }

        if (transactionCommand.Data is IDeleteTagsCommand deleteTagsCommand)
        {
            await TagDocument
                .GetDeleteCommand(transactionCommand, deleteTagsCommand)
                .Execute(_mongoSession, cancellationToken);
        }
    }
}
