using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions;

internal abstract class TransactionRepositoryWrapper : DisposableResourceBaseClass, ITransactionRepository
{
    private readonly ITransactionRepository _transactionRepository;

    protected TransactionRepositoryWrapper(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public Task<Id[]> GetTransactionIds(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetTransactionIds(agentSignatureQuery, cancellationToken));
    }

    public Task<Id[]> GetTransactionIds(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetTransactionIds(commandQuery, cancellationToken));
    }

    public Task<Id[]> GetTransactionIds(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetTransactionIds(leaseQuery, cancellationToken));
    }

    public Task<Id[]> GetTransactionIds(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetTransactionIds(tagQuery, cancellationToken));
    }

    public Task<Id[]> GetEntityIds(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetEntityIds(agentSignatureQuery, cancellationToken));
    }

    public Task<Id[]> GetEntityIds(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetEntityIds(commandQuery, cancellationToken));
    }

    public Task<Id[]> GetEntityIds(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetEntityIds(leaseQuery, cancellationToken));
    }

    public Task<Id[]> GetEntityIds(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetEntityIds(tagQuery, cancellationToken));
    }

    public Task<object[]> GetAgentSignatures(IAgentSignatureQuery agentSignatureQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetAgentSignatures(agentSignatureQuery, cancellationToken));
    }

    public Task<object[]> GetCommands(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetCommands(commandQuery, cancellationToken));
    }

    public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetLeases(leaseQuery, cancellationToken));
    }

    public Task<ITag[]> GetTags(ITagQuery tagQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetTags(tagQuery, cancellationToken));
    }

    public Task<IEntityAnnotation<object>[]> GetAnnotatedCommands(ICommandQuery commandQuery, CancellationToken cancellationToken = default)
    {
        return WrapQuery(_transactionRepository.GetAnnotatedCommands(commandQuery, cancellationToken));
    }

    public Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        return WrapCommand(_transactionRepository.PutTransaction(transaction, cancellationToken));
    }

    public override async ValueTask DisposeAsync()
    {
        await _transactionRepository.DisposeAsync();
    }

    protected abstract Task<T[]> WrapQuery<T>(Task<T[]> task);

    protected abstract Task<bool> WrapCommand(Task<bool> task);
}
