using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Sessions;
using MongoDB.Bson;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal class MongoDbTransactionRepository : DisposableResourceBaseClass, ITransactionRepository
{
    private readonly IMongoSession _mongoSession;
    private readonly IEnvelopeService<BsonDocument> _envelopeService;

    public MongoDbTransactionRepository
    (
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService
    )
    {
        _mongoSession = mongoSession;
        _envelopeService = envelopeService;
    }

    public Task<Id[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .GetTransactionIds(_mongoSession, cancellationToken);
    }

    public Task<Id[]> GetTransactionIds(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .GetTransactionIds(_mongoSession, cancellationToken);
    }

    public Task<Id[]> GetTransactionIds(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .GetTransactionIds(_mongoSession, cancellationToken);
    }

    public Task<Id[]> GetTransactionIds(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .GetTransactionIds(_mongoSession, cancellationToken);
    }

    public Task<Id[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .GetEntitiesIds(_mongoSession, cancellationToken);
    }

    public Task<Id[]> GetEntityIds(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .GetEntityIds(_mongoSession, cancellationToken);
    }

    public Task<Id[]> GetEntityIds(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .GetEntityIds(_mongoSession, cancellationToken);
    }

    public Task<Id[]> GetEntityIds(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .GetEntityIds(_mongoSession, cancellationToken);
    }

    public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .GetData<AgentSignatureDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public Task<object[]> GetCommands(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .GetData<CommandDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .GetData<LeaseDocument, ILease>(_mongoSession, _envelopeService, cancellationToken);
    }

    public Task<ITag[]> GetTags(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .GetData<TagDocument, ITag>(_mongoSession, _envelopeService, cancellationToken);
    }

    public Task<IEntitiesAnnotation<object>[]> GetAnnotatedAgentSignatures(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .GetEntitiesAnnotation<AgentSignatureDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .GetEntityAnnotation<CommandDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    private async Task PutAgentSignature(ITransaction transaction, CancellationToken cancellationToken)
    {
        await AgentSignatureDocument
            .GetInsertCommand(_envelopeService, transaction)
            .Execute(_mongoSession, cancellationToken);
    }

    private async Task PutCommand(ITransaction transaction, IAppendCommandTransactionStep appendCommandTransactionStep, CancellationToken cancellationToken)
    {
        VersionZeroReservedException.ThrowIfZero(appendCommandTransactionStep.EntityVersionNumber);

        var previousVersionNumber = await CommandDocument
            .GetLastEntityVersionNumber(_mongoSession, appendCommandTransactionStep.EntityId, cancellationToken);

        OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber, appendCommandTransactionStep.PreviousEntityVersionNumber);

        await CommandDocument
            .GetInsertCommand(_envelopeService, transaction, appendCommandTransactionStep)
            .Execute(_mongoSession, cancellationToken);
    }

    private async Task PutLeases(ITransaction transaction, IAddLeasesTransactionStep addLeasesTransactionStep, CancellationToken cancellationToken)
    {
        await LeaseDocument
            .GetInsertCommand(_envelopeService, transaction, addLeasesTransactionStep)
            .Execute(_mongoSession, cancellationToken);
    }

    private async Task PutTags(ITransaction transaction, IAddTagsTransactionStep addTagsTransactionStep, CancellationToken cancellationToken)
    {
        await TagDocument
            .GetInsertCommand(_envelopeService, transaction, addTagsTransactionStep)
            .Execute(_mongoSession, cancellationToken);
    }

    private async Task DeleteLeases(IDeleteLeasesTransactionStep deleteLeasesTransactionStep, CancellationToken cancellationToken)
    {
        await LeaseDocument
            .GetDeleteCommand(deleteLeasesTransactionStep)
            .Execute(_mongoSession, cancellationToken);
    }

    private async Task DeleteTags(IDeleteTagsTransactionStep deleteTagsTransactionStep, CancellationToken cancellationToken)
    {
        await TagDocument
            .GetDeleteCommand(deleteTagsTransactionStep)
            .Execute(_mongoSession, cancellationToken);
    }

    public async Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            _mongoSession.StartTransaction();

            await PutAgentSignature(transaction, cancellationToken);

            foreach (var transactionStep in transaction.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await (transactionStep switch
                {
                    IAppendCommandTransactionStep appendCommandTransactionStep
                        => PutCommand(transaction, appendCommandTransactionStep, cancellationToken),
                    IAddLeasesTransactionStep addLeasesTransactionStep
                        => PutLeases(transaction, addLeasesTransactionStep, cancellationToken),
                    IAddTagsTransactionStep addTagsTransactionStep
                        => PutTags(transaction, addTagsTransactionStep, cancellationToken),
                    IDeleteLeasesTransactionStep deleteLeasesTransactionStep
                        => DeleteLeases(deleteLeasesTransactionStep, cancellationToken),
                    IDeleteTagsTransactionStep deleteTagsTransactionStep
                        => DeleteTags(deleteTagsTransactionStep, cancellationToken),
                    _ => throw new NotSupportedException()
                });
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
}
