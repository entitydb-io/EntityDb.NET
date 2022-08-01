using EntityDb.SqlDb.Sessions;

namespace EntityDb.SqlDb.Transactions;

internal class
    TestModeSqlDbTransactionRepositoryFactory<TOptions> : SqlDbTransactionRepositoryFactoryWrapper<TOptions>
    where TOptions : class
{
    private (ISqlDbSession<TOptions> Normal, TestModeSqlDbSession<TOptions> TestMode)? _sessions;

    public TestModeSqlDbTransactionRepositoryFactory(
        ISqlDbTransactionRepositoryFactory<TOptions> sqlDbTransactionRepositoryFactory) : base(sqlDbTransactionRepositoryFactory)
    {
    }

    public override async Task<ISqlDbSession<TOptions>> CreateSession(SqlDbTransactionSessionOptions options,
        CancellationToken cancellationToken)
    {
        if (_sessions.HasValue)
        {
            return _sessions.Value.TestMode
                .WithTransactionSessionOptions(options);
        }

        var normalOptions = new SqlDbTransactionSessionOptions
        {
            ConnectionString = options.ConnectionString
        };

        var normalSession = await base.CreateSession(normalOptions, cancellationToken);

        var testModeSession = new TestModeSqlDbSession<TOptions>(normalSession);

        await normalSession.StartTransaction(cancellationToken);

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
