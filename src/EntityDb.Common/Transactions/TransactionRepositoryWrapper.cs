using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal abstract class TransactionRepositoryWrapper : DisposableResourceBaseClass, ITransactionRepository
{
    private readonly ITransactionRepository _transactionRepository;

    protected TransactionRepositoryWrapper(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public Task<Id[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return WrapQuery(_transactionRepository.GetTransactionIds(agentSignatureQuery));
    }

    public Task<Id[]> GetTransactionIds(ICommandQuery commandQuery)
    {
        return WrapQuery(_transactionRepository.GetTransactionIds(commandQuery));
    }

    public Task<Id[]> GetTransactionIds(ILeaseQuery leaseQuery)
    {
        return WrapQuery(_transactionRepository.GetTransactionIds(leaseQuery));
    }

    public Task<Id[]> GetTransactionIds(ITagQuery tagQuery)
    {
        return WrapQuery(_transactionRepository.GetTransactionIds(tagQuery));
    }

    public Task<Id[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery)
    {
        return WrapQuery(_transactionRepository.GetEntityIds(agentSignatureQuery));
    }

    public Task<Id[]> GetEntityIds(ICommandQuery commandQuery)
    {
        return WrapQuery(_transactionRepository.GetEntityIds(commandQuery));
    }

    public Task<Id[]> GetEntityIds(ILeaseQuery leaseQuery)
    {
        return WrapQuery(_transactionRepository.GetEntityIds(leaseQuery));
    }

    public Task<Id[]> GetEntityIds(ITagQuery tagQuery)
    {
        return WrapQuery(_transactionRepository.GetEntityIds(tagQuery));
    }

    public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery)
    {
        return WrapQuery(_transactionRepository.GetAgentSignatures(agentSignatureQuery));
    }

    public Task<object[]> GetCommands(ICommandQuery commandQuery)
    {
        return WrapQuery(_transactionRepository.GetCommands(commandQuery));
    }

    public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
    {
        return WrapQuery(_transactionRepository.GetLeases(leaseQuery));
    }

    public Task<ITag[]> GetTags(ITagQuery tagQuery)
    {
        return WrapQuery(_transactionRepository.GetTags(tagQuery));
    }

    public Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery)
    {
        return WrapQuery(_transactionRepository.GetAnnotatedCommands(commandQuery));
    }

    public Task<bool> PutTransaction(ITransaction transaction)
    {
        return WrapCommand(_transactionRepository.PutTransaction(transaction));
    }

    public override async ValueTask DisposeAsync()
    {
        await _transactionRepository.DisposeAsync();
    }

    protected abstract Task<T[]> WrapQuery<T>(Task<T[]> task);

    protected abstract Task<bool> WrapCommand(Task<bool> task);
}
