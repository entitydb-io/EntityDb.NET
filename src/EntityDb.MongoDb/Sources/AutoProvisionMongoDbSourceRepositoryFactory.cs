using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Sources.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.MongoDb.Sources;

internal sealed class AutoProvisionMongoDbSourceRepositoryFactory : MongoDbSourceRepositoryFactoryWrapper
{
    private static readonly SemaphoreSlim Lock = new(1);
    private static bool _provisioned;
    private readonly ILogger<AutoProvisionMongoDbSourceRepositoryFactory> _logger;

    public AutoProvisionMongoDbSourceRepositoryFactory(
        ILogger<AutoProvisionMongoDbSourceRepositoryFactory> logger,
        IMongoDbSourceRepositoryFactory mongoDbSourceRepositoryFactory) : base(
        mongoDbSourceRepositoryFactory)
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

    public override async Task<IMongoSession> CreateSession(MongoDbSourceSessionOptions options,
        CancellationToken cancellationToken)
    {
        var mongoSession = await base.CreateSession(options, cancellationToken);

        await AcquireLock(cancellationToken);

        if (_provisioned)
        {
            ReleaseLock();

            return mongoSession;
        }

        await mongoSession.MongoDatabase.Client.ProvisionSourceCollections(mongoSession.MongoDatabase
            .DatabaseNamespace
            .DatabaseName, cancellationToken);

        _provisioned = true;

        _logger.LogInformation("MongoDb has been auto-provisioned");

        ReleaseLock();

        return mongoSession;
    }

    public static IMongoDbSourceRepositoryFactory Create(IServiceProvider serviceProvider,
        IMongoDbSourceRepositoryFactory mongoDbSourceRepositoryFactory)
    {
        return ActivatorUtilities.CreateInstance<AutoProvisionMongoDbSourceRepositoryFactory>(serviceProvider,
            mongoDbSourceRepositoryFactory);
    }
}
