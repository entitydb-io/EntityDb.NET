using EntityDb.MongoDb.Sessions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal class
    TestModeMongoDbTransactionRepositoryFactory : MongoDbTransactionRepositoryFactoryWrapper
{
    private (IMongoSession Normal, TestModeMongoSession TestMode)? _sessions;

    public TestModeMongoDbTransactionRepositoryFactory(
        IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory) : base(
        mongoDbTransactionRepositoryFactory)
    {
    }

    public override async Task<IMongoSession> CreateSession(MongoDbTransactionSessionOptions options,
        CancellationToken cancellationToken)
    {
        if (_sessions.HasValue)
        {
            return _sessions.Value.TestMode
                .WithTransactionSessionOptions(options);
        }

        var normalOptions = new MongoDbTransactionSessionOptions
        {
            ConnectionString = options.ConnectionString,
            DatabaseName = options.DatabaseName,
        };

        var normalSession = await base.CreateSession(normalOptions, cancellationToken);

        var testModeSession = new TestModeMongoSession(normalSession);

        normalSession.StartTransaction();

        _sessions = (normalSession, testModeSession);

        return _sessions.Value.TestMode
            .WithTransactionSessionOptions(options);
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
