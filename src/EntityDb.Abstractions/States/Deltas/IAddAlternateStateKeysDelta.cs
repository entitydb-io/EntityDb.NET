using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Abstractions.States.Deltas;

public interface IAddAlternateStateKeysDelta<TState>
{
    IEnumerable<IStateKey> GetAlternateStateKeys(TState state);
}
