using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.States;

internal abstract class StateRepositoryWrapper<TState> : DisposableResourceBaseClass,
    IStateRepository<TState>
{
    private readonly IStateRepository<TState> _stateRepository;

    protected StateRepositoryWrapper
    (
        IStateRepository<TState> stateRepository
    )
    {
        _stateRepository = stateRepository;
    }

    public virtual Task<bool> Put(Pointer statePointer, TState state,
        CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _stateRepository.Put(statePointer, state, cancellationToken));
    }

    public virtual Task<TState?> Get(Pointer statePointer,
        CancellationToken cancellationToken = default)
    {
        return WrapQuery(() => _stateRepository.Get(statePointer, cancellationToken));
    }

    public virtual Task<bool> Delete(Pointer[] statePointers, CancellationToken cancellationToken = default)
    {
        return WrapCommand(() => _stateRepository.Delete(statePointers, cancellationToken));
    }

    public override async ValueTask DisposeAsync()
    {
        await _stateRepository.DisposeAsync();
    }

    protected abstract Task<TState?> WrapQuery(Func<Task<TState?>> task);

    protected abstract Task<bool> WrapCommand(Func<Task<bool>> task);
}
