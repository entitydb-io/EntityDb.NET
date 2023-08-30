using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.EntityFramework.Sessions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.EntityFramework.Snapshots;

internal class TestModeEntityFrameworkSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, IEntityFrameworkSnapshotRepositoryFactory<TSnapshot>
{
    private readonly ILogger<TestModeEntityFrameworkSnapshotRepositoryFactory<TSnapshot>> _logger;
    private readonly IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> _entityFrameworkSnapshotRepositoryFactory;

    private (IEntityFrameworkSession<TSnapshot> Normal, TestModeEntityFrameworkSession<TSnapshot> TestMode)? _sessions;

    public TestModeEntityFrameworkSnapshotRepositoryFactory
    (
        ILogger<TestModeEntityFrameworkSnapshotRepositoryFactory<TSnapshot>> logger,
        IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> entityFrameworkSnapshotRepositoryFactory
    )
    {
        _logger = logger;
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
            ConnectionString = options.ConnectionString,
            KeepSnapshotsWithoutSnapshotReferences = options.KeepSnapshotsWithoutSnapshotReferences,
        };

        var normalSession = await _entityFrameworkSnapshotRepositoryFactory.CreateSession(normalOptions, cancellationToken);

        try
        {
            var databaseCreator = (RelationalDatabaseCreator)normalSession.DbContext.Database.GetService<IDatabaseCreator>();

            await databaseCreator.CreateTablesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "It looks like the database tables have already been created");
        }

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

    public EntityFrameworkSnapshotSessionOptions GetSessionOptions(string snapshotSessionOptionsName)
    {
        return _entityFrameworkSnapshotRepositoryFactory.GetSessionOptions(snapshotSessionOptionsName);
    }

    public static TestModeEntityFrameworkSnapshotRepositoryFactory<TSnapshot> Create(IServiceProvider serviceProvider, IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> entityFrameworkSnapshotRepositoryFactory)
    {
        return ActivatorUtilities.CreateInstance<TestModeEntityFrameworkSnapshotRepositoryFactory<TSnapshot>>(serviceProvider, entityFrameworkSnapshotRepositoryFactory);
    }
}
