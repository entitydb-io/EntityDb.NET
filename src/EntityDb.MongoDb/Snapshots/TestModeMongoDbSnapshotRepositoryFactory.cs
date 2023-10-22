using EntityDb.MongoDb.Snapshots.Sessions;

namespace EntityDb.MongoDb.Snapshots;

internal class TestModeMongoDbSnapshotRepositoryFactory<TSnapshot> : MongoDbSnapshotRepositoryFactoryWrapper<TSnapshot>
{
    private (IMongoSession Normal, TestModeMongoSession TestMode)? _sessions;

    public TestModeMongoDbSnapshotRepositoryFactory
    (
        IMongoDbSnapshotRepositoryFactory<TSnapshot> mongoDbSnapshotRepositoryFactory
    )
        : base(mongoDbSnapshotRepositoryFactory)
    {
    }

    public override async Task<IMongoSession> CreateSession(MongoDbSnapshotSessionOptions options,
        CancellationToken cancellationToken)
    {
        if (_sessions.HasValue)
        {
            return _sessions.Value.TestMode
                .WithSessionOptions(options);
        }

        var normalOptions = new MongoDbSnapshotSessionOptions
        {
            ConnectionString = options.ConnectionString,
            DatabaseName = options.DatabaseName,
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
