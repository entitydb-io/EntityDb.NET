using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.EntityFramework.Sessions;

namespace EntityDb.EntityFramework.Snapshots;

internal class TestModeEntityFrameworkSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, IEntityFrameworkSnapshotRepositoryFactory<TSnapshot>
{
    private readonly IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> _entityFrameworkSnapshotRepositoryFactory;

    private (IEntityFrameworkSession<TSnapshot> Normal, TestModeEntityFrameworkSession<TSnapshot> TestMode)? _sessions;

    public TestModeEntityFrameworkSnapshotRepositoryFactory(
        IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> entityFrameworkSnapshotRepositoryFactory)
    {
        _entityFrameworkSnapshotRepositoryFactory = entityFrameworkSnapshotRepositoryFactory;
    }

    public ISnapshotRepository<TSnapshot> CreateRepository(IEntityFrameworkSession<TSnapshot> entityFrameworkSession)
    {
        return _entityFrameworkSnapshotRepositoryFactory.CreateRepository(entityFrameworkSession);
    }

    public async Task<IEntityFrameworkSession<TSnapshot>> CreateSession(EntityFrameworkSnapshotSessionOptions options,
        CancellationToken cancellationToken)
    {
        if (_sessions.HasValue)
        {
            return _sessions.Value.TestMode
                .WithSnapshotSessionOptions(options);
        }

        var normalOptions = new EntityFrameworkSnapshotSessionOptions
        {
            ReadOnly = false
        };

        var normalSession = await _entityFrameworkSnapshotRepositoryFactory.CreateSession(normalOptions, cancellationToken);

        var testModeSession = new TestModeEntityFrameworkSession<TSnapshot>(normalSession);

        await normalSession.StartTransaction(default);

        _sessions = (normalSession, testModeSession);

        return _sessions.Value.TestMode
            .WithSnapshotSessionOptions(options);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_sessions.HasValue)
        {
            await _sessions.Value.Normal.AbortTransaction(default);
            await _sessions.Value.Normal.DisposeAsync();
        }
    }

    public EntityFrameworkSnapshotSessionOptions GetTransactionSessionOptions(string snapshotSessionOptionsName)
    {
        return _entityFrameworkSnapshotRepositoryFactory.GetTransactionSessionOptions(snapshotSessionOptionsName);
    }
}
