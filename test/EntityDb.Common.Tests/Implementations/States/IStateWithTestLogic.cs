using EntityDb.Abstractions.States;

namespace EntityDb.Common.Tests.Implementations.States;

public interface IStateWithTestLogic<TState> : IState<TState>
    where TState : class
{
    static abstract string MongoDbCollectionName { get; }
    static abstract string RedisKeyNamespace { get; }
    static abstract AsyncLocal<Func<TState, bool>?> ShouldPersistLogic { get; }
    static abstract AsyncLocal<Func<TState, bool>?> ShouldPersistAsLatestLogic { get; }
}
