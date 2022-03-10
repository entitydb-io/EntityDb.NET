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

    public Task<Id[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .GetTransactionIds(_mongoSession);
    }

    public Task<Id[]> GetTransactionIds(ICommandQuery commandQuery)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .GetTransactionIds(_mongoSession);
    }

    public Task<Id[]> GetTransactionIds(ILeaseQuery leaseQuery)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .GetTransactionIds(_mongoSession);
    }

    public Task<Id[]> GetTransactionIds(ITagQuery tagQuery)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .GetTransactionIds(_mongoSession);
    }

    public Task<Id[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .GetEntitiesIds(_mongoSession);
    }

    public Task<Id[]> GetEntityIds(ICommandQuery commandQuery)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .GetEntityIds(_mongoSession);
    }

    public Task<Id[]> GetEntityIds(ILeaseQuery leaseQuery)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .GetEntityIds(_mongoSession);
    }

    public Task<Id[]> GetEntityIds(ITagQuery tagQuery)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .GetEntityIds(_mongoSession);
    }

    public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .GetData<AgentSignatureDocument, object>(_mongoSession, _envelopeService);
    }

    public Task<object[]> GetCommands(ICommandQuery commandQuery)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .GetData<CommandDocument, object>(_mongoSession, _envelopeService);
    }

    public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .GetData<LeaseDocument, ILease>(_mongoSession, _envelopeService);
    }

    public Task<ITag[]> GetTags(ITagQuery tagQuery)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .GetData<TagDocument, ITag>(_mongoSession, _envelopeService);
    }

    public Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .GetEntityAnnotation<CommandDocument, object>(_mongoSession, _envelopeService);
    }

    private async Task PutAgentSignature(ITransaction transaction)
    {
        await AgentSignatureDocument
            .GetInsertCommand(_envelopeService, transaction)
            .Execute(_mongoSession);
    }
    
    private async Task PutCommand(ITransaction transaction, IAppendCommandTransactionStep appendCommandTransactionStep)
    {
        VersionZeroReservedException.ThrowIfZero(appendCommandTransactionStep.EntityVersionNumber);

        var previousVersionNumber = await CommandDocument
            .GetLastEntityVersionNumber(_mongoSession, appendCommandTransactionStep.EntityId);

        OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber, appendCommandTransactionStep.PreviousEntityVersionNumber);

        await CommandDocument
            .GetInsertCommand(_envelopeService, transaction, appendCommandTransactionStep)
            .Execute(_mongoSession);
    }

    private async Task PutLeases(ITransaction transaction, IAddLeasesTransactionStep addLeasesTransactionStep)
    {
        await LeaseDocument
            .GetInsertCommand(_envelopeService, transaction, addLeasesTransactionStep)
            .Execute(_mongoSession);
    }

    private async Task PutTags(ITransaction transaction, IAddTagsTransactionStep addTagsTransactionStep)
    {
        await TagDocument
            .GetInsertCommand(_envelopeService, transaction, addTagsTransactionStep)
            .Execute(_mongoSession);
    }

    private async Task DeleteLeases(IDeleteLeasesTransactionStep deleteLeasesTransactionStep)
    {
        await LeaseDocument
            .GetDeleteCommand(deleteLeasesTransactionStep)
            .Execute(_mongoSession);
    }

    private async Task DeleteTags(IDeleteTagsTransactionStep deleteTagsTransactionStep)
    {
        await TagDocument
            .GetDeleteCommand(deleteTagsTransactionStep)
            .Execute(_mongoSession);
    }

    public async Task<bool> PutTransaction(ITransaction transaction)
    {
        try
        {
            _mongoSession.StartTransaction();

            await PutAgentSignature(transaction);

            foreach (var transactionStep in transaction.Steps)
            {
                await (transactionStep switch
                {
                    IAppendCommandTransactionStep appendCommandTransactionStep => PutCommand(transaction, appendCommandTransactionStep),
                    IAddLeasesTransactionStep addLeasesTransactionStep => PutLeases(transaction, addLeasesTransactionStep),
                    IAddTagsTransactionStep addTagsTransactionStep => PutTags(transaction, addTagsTransactionStep),
                    IDeleteLeasesTransactionStep deleteLeasesTransactionStep => DeleteLeases(deleteLeasesTransactionStep),
                    IDeleteTagsTransactionStep deleteTagsTransactionStep => DeleteTags(deleteTagsTransactionStep),
                    _ => throw new NotSupportedException()
                });
            }

            await _mongoSession.CommitTransaction();

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
