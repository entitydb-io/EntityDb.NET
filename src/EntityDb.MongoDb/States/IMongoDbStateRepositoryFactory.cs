using EntityDb.Abstractions.States;
using EntityDb.MongoDb.States.Sessions;

namespace EntityDb.MongoDb.States;

internal interface IMongoDbStateRepositoryFactory<TState> : IStateRepositoryFactory<TState>
{
    async Task<IStateRepository<TState>> IStateRepositoryFactory<TState>.Create(
        string stateSessionOptionsName, CancellationToken cancellationToken)
    {
        var options = GetSessionOptions(stateSessionOptionsName);

        var mongoSession = await CreateSession(options, cancellationToken);

        return CreateRepository(mongoSession);
    }

    MongoDbStateSessionOptions GetSessionOptions(string stateSessionOptionsName);

    Task<IMongoSession> CreateSession(MongoDbStateSessionOptions options,
        CancellationToken cancellationToken);

    IStateRepository<TState> CreateRepository(IMongoSession mongoSession);
}
