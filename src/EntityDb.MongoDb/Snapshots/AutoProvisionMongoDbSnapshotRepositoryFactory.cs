using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Snapshots.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.MongoDb.Snapshots;

internal sealed class
    AutoProvisionMongoDbSnapshotRepositoryFactory<TSnapshot> : MongoDbSnapshotRepositoryFactoryWrapper<TSnapshot>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly SemaphoreSlim Lock = new(1);

    // ReSharper disable once StaticMemberInGenericType
    private static bool _provisioned;

    private readonly ILogger<AutoProvisionMongoDbSnapshotRepositoryFactory<TSnapshot>> _logger;

    public AutoProvisionMongoDbSnapshotRepositoryFactory
    (
        ILogger<AutoProvisionMongoDbSnapshotRepositoryFactory<TSnapshot>> logger,
        IMongoDbSnapshotRepositoryFactory<TSnapshot> mongoDbSnapshotRepositoryFactory
    )
        : base(mongoDbSnapshotRepositoryFactory)
    {
        _logger = logger;
    }

    private async Task AcquireLock(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Wait for MongoDb Auto-Provisioning Lock");

        await Lock.WaitAsync(cancellationToken);

        _logger.LogInformation("MongoDb Auto-Provisioning Lock Acquired");
    }

    private void ReleaseLock()
    {
        _logger.LogInformation("Release MongoDb Auto-Provisioning Lock");

        Lock.Release();

        _logger.LogInformation("MongoDb Auto-Provisioning Lock Released");
    }

    public override async Task<IMongoSession> CreateSession(MongoDbSnapshotSessionOptions options,
        CancellationToken cancellationToken)
    {
        var mongoSession = await base.CreateSession(options, cancellationToken);

        await AcquireLock(cancellationToken);

        if (_provisioned)
        {
            ReleaseLock();

            return mongoSession;
        }

        await mongoSession.MongoDatabase.Client.ProvisionSnapshotCollection
        (
            mongoSession.MongoDatabase.DatabaseNamespace.DatabaseName,
            mongoSession.CollectionName,
            cancellationToken
        );

        _provisioned = true;

        _logger.LogInformation("MongoDb has been auto-provisioned");

        ReleaseLock();

        return mongoSession;
    }

    public static IMongoDbSnapshotRepositoryFactory<TSnapshot> Create(IServiceProvider serviceProvider,
        IMongoDbSnapshotRepositoryFactory<TSnapshot> mongoDbSnapshotRepositoryFactory)
    {
        return ActivatorUtilities.CreateInstance<AutoProvisionMongoDbSnapshotRepositoryFactory<TSnapshot>>(
            serviceProvider,
            mongoDbSnapshotRepositoryFactory);
    }
}
