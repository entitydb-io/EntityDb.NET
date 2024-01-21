using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.States.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.MongoDb.States;

internal sealed class
    AutoProvisionMongoDbStateRepositoryFactory<TState> : MongoDbStateRepositoryFactoryWrapper<TState>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly SemaphoreSlim Lock = new(1);

    // ReSharper disable once StaticMemberInGenericType
    private static bool _provisioned;

    private readonly ILogger<AutoProvisionMongoDbStateRepositoryFactory<TState>> _logger;

    public AutoProvisionMongoDbStateRepositoryFactory
    (
        ILogger<AutoProvisionMongoDbStateRepositoryFactory<TState>> logger,
        IMongoDbStateRepositoryFactory<TState> mongoDbStateRepositoryFactory
    )
        : base(mongoDbStateRepositoryFactory)
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

    public override async Task<IMongoSession> CreateSession(MongoDbStateSessionOptions options,
        CancellationToken cancellationToken)
    {
        var mongoSession = await base.CreateSession(options, cancellationToken);

        await AcquireLock(cancellationToken);

        if (_provisioned)
        {
            ReleaseLock();

            return mongoSession;
        }

        await mongoSession.MongoDatabase.Client.ProvisionStateCollection
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

    public static IMongoDbStateRepositoryFactory<TState> Create(IServiceProvider serviceProvider,
        IMongoDbStateRepositoryFactory<TState> mongoDbStateRepositoryFactory)
    {
        return ActivatorUtilities.CreateInstance<AutoProvisionMongoDbStateRepositoryFactory<TState>>(
            serviceProvider,
            mongoDbStateRepositoryFactory);
    }
}
