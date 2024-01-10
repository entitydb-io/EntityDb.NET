using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.States;

internal sealed class TestModeStateRepository<TState> : DisposableResourceBaseClass,
    IStateRepository<TState>
{
    private readonly IStateRepository<TState> _stateRepository;
    private readonly TestModeStateManager<TState> _testModeStateManager;

    public TestModeStateRepository
    (
        IStateRepository<TState> stateRepository,
        TestModeStateManager<TState> testModeStateManager
    )
    {
        _stateRepository = stateRepository;
        _testModeStateManager = testModeStateManager;
    }

    public Task<bool> Put(Pointer statePointer, TState state,
        CancellationToken cancellationToken = default)
    {
        _testModeStateManager.AddStatePointer(this, statePointer);

        return _stateRepository.Put(statePointer, state, cancellationToken);
    }

    public Task<TState?> Get(Pointer statePointer, CancellationToken cancellationToken = default)
    {
        return _stateRepository.Get(statePointer, cancellationToken);
    }

    public Task<bool> Delete(Pointer[] statePointers, CancellationToken cancellationToken = default)
    {
        _testModeStateManager.RemoveStatePointers(this, statePointers);

        return _stateRepository.Delete(statePointers, cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await _stateRepository.DisposeAsync();
    }
}
