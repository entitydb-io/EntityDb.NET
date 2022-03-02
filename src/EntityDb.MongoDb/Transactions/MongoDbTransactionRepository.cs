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

internal class MongoDbTransactionRepository<TEntity> : DisposableResourceBaseClass, ITransactionRepository<TEntity>
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

    private async Task ExecuteCommandTransactionStep(ITransaction<TEntity> transaction, ICommandTransactionStep<TEntity> commandTransactionStep)
    {
        VersionZeroReservedException.ThrowIfZero(commandTransactionStep.NextEntityVersionNumber);

        var previousVersionNumber = await CommandDocument.GetLastEntityVersionNumber(_mongoSession, commandTransactionStep.EntityId);

        OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber, commandTransactionStep.PreviousEntityVersionNumber);

        await CommandDocument.GetInsertCommand<TEntity>(_mongoSession).Execute(transaction, commandTransactionStep);
    }

    private async Task ExecuteLeaseTransactionStep(ITransaction<TEntity> transaction, ILeaseTransactionStep<TEntity> leaseTransactionStep)
    {
        await LeaseDocument.GetDeleteCommand<TEntity>(_mongoSession).Execute(transaction, leaseTransactionStep);

        await LeaseDocument.GetInsertCommand<TEntity>(_mongoSession).Execute(transaction, leaseTransactionStep);
    }

    private async Task ExecuteTagTransactionStep(ITransaction<TEntity> transaction, ITagTransactionStep<TEntity> tagTransactionStep)
    {
        await TagDocument.GetDeleteCommand<TEntity>(_mongoSession).Execute(transaction, tagTransactionStep);

        await TagDocument.GetInsertCommand<TEntity>(_mongoSession).Execute(transaction, tagTransactionStep);
    }

    public async Task<bool> PutTransaction(ITransaction<TEntity> transaction)
    {
        try
        {
            _mongoSession.StartTransaction();

            await AgentSignatureDocument
                .GetInsertCommand<TEntity>(_mongoSession)
                .Execute(transaction);

            foreach (var transactionStep in transaction.Steps)
            {
                if (transactionStep is ICommandTransactionStep<TEntity> commandTransactionStep)
                {
                    await ExecuteCommandTransactionStep(transaction, commandTransactionStep);
                }

                if (transactionStep is ILeaseTransactionStep<TEntity> leaseTransactionStep)
                {
                    await ExecuteLeaseTransactionStep(transaction, leaseTransactionStep);
                }

                if (transactionStep is ITagTransactionStep<TEntity> tagTransactionStep)
                {
                    await ExecuteTagTransactionStep(transaction, tagTransactionStep);
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
