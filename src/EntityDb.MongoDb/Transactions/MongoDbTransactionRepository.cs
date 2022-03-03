using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Sessions;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal class MongoDbTransactionRepository : DisposableResourceBaseClass, ITransactionRepository
{
    private readonly IMongoSession _mongoSession;

    public MongoDbTransactionRepository
    (
        IMongoSession mongoSession
    )
    {
        _mongoSession = mongoSession;
    }

    public Task<Guid[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return AgentSignatureDocument
            .GetQuery(_mongoSession, agentSignatureQuery)
            .GetTransactionIds();
    }

    public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
    {
        return CommandDocument
            .GetQuery(_mongoSession, commandQuery)
            .GetTransactionIds();
    }

    public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
    {
        return LeaseDocument
            .GetQuery(_mongoSession, leaseQuery)
            .GetTransactionIds();
    }

    public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
    {
        return TagDocument
            .GetQuery(_mongoSession, tagQuery)
            .GetTransactionIds();
    }

    public Task<Guid[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return AgentSignatureDocument
            .GetQuery(_mongoSession, agentSignatureQuery)
            .GetEntitiesIds();
    }

    public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
    {
        return CommandDocument
            .GetQuery(_mongoSession, commandQuery)
            .GetEntityIds();
    }

    public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
    {
        return LeaseDocument
            .GetQuery(_mongoSession, leaseQuery)
            .GetEntityIds();
    }

    public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
    {
        return TagDocument
            .GetQuery(_mongoSession, tagQuery)
            .GetEntityIds();
    }

    public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery)
    {
        return AgentSignatureDocument
            .GetQuery(_mongoSession, agentSignatureQuery)
            .GetData<AgentSignatureDocument, object>();
    }

    public Task<object[]> GetCommands(ICommandQuery commandQuery)
    {
        return CommandDocument
            .GetQuery(_mongoSession, commandQuery)
            .GetData<CommandDocument, object>();
    }

    public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
    {
        return LeaseDocument
            .GetQuery(_mongoSession, leaseQuery)
            .GetData<LeaseDocument, ILease>();
    }

    public Task<ITag[]> GetTags(ITagQuery tagQuery)
    {
        return TagDocument
            .GetQuery(_mongoSession, tagQuery)
            .GetData<TagDocument, ITag>();
    }

    public Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
    {
        return CommandDocument
            .GetQuery(_mongoSession, commandQuery)
            .GetEntityAnnotation<CommandDocument, object>();
    }

    private DocumentsCommand ToDocumentsCommand(ITransaction transaction)
    {
        return AgentSignatureDocument.GetInsertCommand(_mongoSession, transaction);
    }
    
    private async Task<DocumentsCommand> ToDocumentsCommand(ITransaction transaction, IAppendCommandTransactionStep appendCommandTransactionStep)
    {
        VersionZeroReservedException.ThrowIfZero(appendCommandTransactionStep.EntityVersionNumber);

        var previousVersionNumber = await CommandDocument.GetLastEntityVersionNumber(_mongoSession, appendCommandTransactionStep.EntityId);

        OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber, appendCommandTransactionStep.PreviousEntityVersionNumber);

        return CommandDocument.GetInsertCommand(_mongoSession, transaction, appendCommandTransactionStep);
    }

    private DocumentsCommand ToDocumentsCommand(ITransaction transaction, IAddLeasesTransactionStep addLeasesTransactionStep)
    {
        return LeaseDocument
            .GetInsertCommand(_mongoSession, transaction, addLeasesTransactionStep);
    }

    private DocumentsCommand ToDocumentsCommand(ITransaction transaction, IAddTagsTransactionStep addTagsTransactionStep)
    {
        return TagDocument
            .GetInsertCommand(_mongoSession, transaction, addTagsTransactionStep);
    }

    private DocumentsCommand ToDocumentsCommand(IDeleteLeasesTransactionStep deleteLeasesTransactionStep)
    {
        return LeaseDocument
            .GetDeleteCommand(_mongoSession, deleteLeasesTransactionStep);
    }

    private DocumentsCommand ToDocumentsCommand(IDeleteTagsTransactionStep deleteTagsTransactionStep)
    {
        return TagDocument
            .GetDeleteCommand(_mongoSession, deleteTagsTransactionStep);
    }

    public async Task<bool> PutTransaction(ITransaction transaction)
    {
        try
        {
            _mongoSession.StartTransaction();

            await ToDocumentsCommand(transaction).Execute();

            foreach (var transactionStep in transaction.Steps)
            {
                var documentsCommand = transactionStep switch
                {
                    IAppendCommandTransactionStep commandTransactionStep =>
                        await ToDocumentsCommand(transaction, commandTransactionStep),

                    IAddLeasesTransactionStep addLeasesTransactionStep =>
                        ToDocumentsCommand(transaction, addLeasesTransactionStep),

                    IAddTagsTransactionStep addTagsTransactionStep =>
                        ToDocumentsCommand(transaction, addTagsTransactionStep),

                    IDeleteLeasesTransactionStep deleteLeasesTransactionStep =>
                        ToDocumentsCommand(deleteLeasesTransactionStep),

                    IDeleteTagsTransactionStep deleteTagsTransactionStep =>
                        ToDocumentsCommand(deleteTagsTransactionStep),

                    _ => throw new NotSupportedException()
                };

                await documentsCommand.Execute();
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
