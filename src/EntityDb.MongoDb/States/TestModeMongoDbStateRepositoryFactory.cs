using EntityDb.MongoDb.States.Sessions;

namespace EntityDb.MongoDb.States;

internal sealed class TestModeMongoDbStateRepositoryFactory<TState> : MongoDbStateRepositoryFactoryWrapper<TState>
{
    private (IMongoSession Normal, TestModeMongoSession TestMode)? _sessions;

    public TestModeMongoDbStateRepositoryFactory
    (
        IMongoDbStateRepositoryFactory<TState> mongoDbStateRepositoryFactory
    )
        : base(mongoDbStateRepositoryFactory)
    {
    }

    public override async Task<IMongoSession> CreateSession(MongoDbStateSessionOptions options,
        CancellationToken cancellationToken)
    {
        if (_sessions.HasValue)
        {
            return _sessions.Value.TestMode
                .WithSessionOptions(options);
        }

        var normalOptions = new MongoDbStateSessionOptions
        {
            ConnectionString = options.ConnectionString, DatabaseName = options.DatabaseName,
        };

        var normalSession = await base.CreateSession(normalOptions, cancellationToken);

        var testModeSession = new TestModeMongoSession(normalSession);

        normalSession.StartTransaction();

        _sessions = (normalSession, testModeSession);

        return _sessions.Value.TestMode
            .WithSessionOptions(options);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_sessions.HasValue)
        {
            await _sessions.Value.Normal.AbortTransaction();
            await _sessions.Value.Normal.DisposeAsync();
        }
    }
}
