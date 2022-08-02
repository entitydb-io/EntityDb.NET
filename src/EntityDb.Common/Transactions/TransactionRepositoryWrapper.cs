using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.Transactions;

internal abstract class TransactionRepositoryWrapper : DisposableResourceBaseClass, ITransactionRepository
{
    private readonly ITransactionRepository _transactionRepository;

    protected TransactionRepositoryWrapper(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateTransactionIds(agentSignatureQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateTransactionIds(commandQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateTransactionIds(leaseQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateTransactionIds(tagQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateEntityIds(agentSignatureQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateEntityIds(commandQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateEntityIds(leaseQuery, cancellationToken));
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateEntityIds(tagQuery, cancellationToken));
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateAgentSignatures(agentSignatureQuery, cancellationToken));
    }

    public IAsyncEnumerable<object> EnumerateCommands(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateCommands(commandQuery, cancellationToken));
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateLeases(leaseQuery, cancellationToken));
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateTags(tagQuery, cancellationToken));
    }

    public IAsyncEnumerable<IEntitiesAnnotation<object>> EnumerateAnnotatedAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateAnnotatedAgentSignatures(agentSignatureQuery, cancellationToken));
    }

    public IAsyncEnumerable<IEntityAnnotation<object>> EnumerateAnnotatedCommands(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _transactionRepository.EnumerateAnnotatedCommands(commandQuery, cancellationToken));
    }

    public Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _transactionRepository.PutTransaction(transaction, cancellationToken));
    }

    public override async ValueTask DisposeAsync()
    {
        await _transactionRepository.DisposeAsync();
    }

    protected abstract IAsyncEnumerable<T> WrapQuery<T>(Func<IAsyncEnumerable<T>> enumerable);

    protected abstract Task<bool> WrapCommand(Func<Task<bool>> task);
}
