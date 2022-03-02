using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
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

    private async Task ExecuteCommandTransactionStep(ITransaction transaction, ICommandTransactionStep commandTransactionStep)
    {
        VersionZeroReservedException.ThrowIfZero(commandTransactionStep.NextEntityVersionNumber);

        var previousVersionNumber = await CommandDocument.GetLastEntityVersionNumber(_mongoSession, commandTransactionStep.EntityId);

        OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber, commandTransactionStep.PreviousEntityVersionNumber);

        await CommandDocument.GetInsertCommand(_mongoSession).Execute(transaction, commandTransactionStep);
    }

    private async Task ExecuteLeaseTransactionStep(ITransaction transaction, ILeaseTransactionStep leaseTransactionStep)
    {
        await LeaseDocument.GetDeleteCommand(_mongoSession).Execute(transaction, leaseTransactionStep);

        await LeaseDocument.GetInsertCommand(_mongoSession).Execute(transaction, leaseTransactionStep);
    }

    private async Task ExecuteTagTransactionStep(ITransaction transaction, ITagTransactionStep tagTransactionStep)
    {
        await TagDocument.GetDeleteCommand(_mongoSession).Execute(transaction, tagTransactionStep);

        await TagDocument.GetInsertCommand(_mongoSession).Execute(transaction, tagTransactionStep);
    }

    public async Task<bool> PutTransaction(ITransaction transaction)
    {
        try
        {
            _mongoSession.StartTransaction();

            await AgentSignatureDocument
                .GetInsertCommand(_mongoSession)
                .Execute(transaction);

            foreach (var transactionStep in transaction.Steps)
            {
                switch (transactionStep)
                {
                    case ICommandTransactionStep commandTransactionStep:
                        await ExecuteCommandTransactionStep(transaction, commandTransactionStep);
                        break;
                    
                    case ILeaseTransactionStep leaseTransactionStep:
                        await ExecuteLeaseTransactionStep(transaction, leaseTransactionStep);
                        break;
                    
                    case ITagTransactionStep tagTransactionStep:
                        await ExecuteTagTransactionStep(transaction, tagTransactionStep);
                        break;
                }
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
