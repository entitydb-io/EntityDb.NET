using EntityDb.MongoDb.Sources.Sessions;

namespace EntityDb.MongoDb.Sources;

internal sealed class TestModeMongoDbSourceRepositoryFactory : MongoDbSourceRepositoryFactoryWrapper
{
    private (IMongoSession Normal, TestModeMongoSession TestMode)? _sessions;

    public TestModeMongoDbSourceRepositoryFactory(
        IMongoDbSourceRepositoryFactory mongoDbSourceRepositoryFactory) : base(
        mongoDbSourceRepositoryFactory)
    {
    }

    public override async Task<IMongoSession> CreateSession(MongoDbSourceSessionOptions options,
        CancellationToken cancellationToken)
    {
        if (_sessions.HasValue)
        {
            return _sessions.Value.TestMode
                .WithSourceSessionOptions(options);
        }

        var normalOptions = new MongoDbSourceSessionOptions
        {
            ConnectionString = options.ConnectionString, DatabaseName = options.DatabaseName,
        };

        var normalSession = await base.CreateSession(normalOptions, cancellationToken);

        var testModeSession = new TestModeMongoSession(normalSession);

        normalSession.StartTransaction();

        _sessions = (normalSession, testModeSession);

        return _sessions.Value.TestMode
            .WithSourceSessionOptions(options);
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
