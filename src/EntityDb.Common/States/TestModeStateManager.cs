using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.States;

internal sealed class TestModeStateManager<TState> : DisposableResourceBaseClass
{
    private readonly Dictionary<IStateRepository<TState>, List<Pointer>> _dictionary = new();

    private List<Pointer> GetStoredStatePointers(IStateRepository<TState> stateRepository)
    {
        if (_dictionary.TryGetValue(stateRepository, out var storedStatePointers))
        {
            return storedStatePointers;
        }

        storedStatePointers = new List<Pointer>();

        _dictionary.Add(stateRepository, storedStatePointers);

        return storedStatePointers;
    }

    public void AddStatePointer(IStateRepository<TState> stateRepository, Pointer statePointer)
    {
        var storedStatePointers = GetStoredStatePointers(stateRepository);

        storedStatePointers.Add(statePointer);
    }

    public void RemoveStatePointers(IStateRepository<TState> stateRepository,
        IEnumerable<Pointer> statePointers)
    {
        var storedStatePointers = GetStoredStatePointers(stateRepository);

        storedStatePointers.RemoveAll(statePointers.Contains);

        if (storedStatePointers.Count == 0)
        {
            _dictionary.Remove(stateRepository);
        }
    }

    /// <remarks>
    ///     This should only be called by the state repository factory.
    /// </remarks>
    public override async ValueTask DisposeAsync()
    {
        foreach (var (stateRepository, storedStatePointers) in _dictionary.ToArray())
        {
            await stateRepository.Delete(storedStatePointers.ToArray());
        }
    }
}
