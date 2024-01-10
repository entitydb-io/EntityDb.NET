using EntityDb.Abstractions.States;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.States;

internal sealed class TestModeStateRepositoryFactory<TState> : DisposableResourceBaseClass,
    IStateRepositoryFactory<TState>
{
    private readonly IStateRepositoryFactory<TState> _stateRepositoryFactory;
    private readonly TestModeStateManager<TState> _testModeStateManager = new();

    public TestModeStateRepositoryFactory
    (
        IStateRepositoryFactory<TState> stateRepositoryFactory
    )
    {
        _stateRepositoryFactory = stateRepositoryFactory;
    }

    public async Task<IStateRepository<TState>> Create(string stateSessionOptionsName,
        CancellationToken cancellationToken = default)
    {
        var stateRepository =
            await _stateRepositoryFactory.Create(stateSessionOptionsName, cancellationToken);

        return new TestModeStateRepository<TState>(stateRepository, _testModeStateManager);
    }

    public override async ValueTask DisposeAsync()
    {
        await _testModeStateManager.DisposeAsync();
        await _stateRepositoryFactory.DisposeAsync();
    }
}
