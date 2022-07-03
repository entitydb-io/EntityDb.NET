using EntityDb.MongoDb.Provisioner.Extensions;
using EntityDb.MongoDb.Sessions;
using EntityDb.MongoDb.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Transactions;

internal sealed class
    AutoProvisionMongoDbTransactionRepositoryFactory : MongoDbTransactionRepositoryFactoryWrapper
{
    private static readonly SemaphoreSlim Lock = new(1);
    private static bool _provisioned;
    private readonly ILogger<AutoProvisionMongoDbTransactionRepositoryFactory> _logger;

    public AutoProvisionMongoDbTransactionRepositoryFactory(
        ILogger<AutoProvisionMongoDbTransactionRepositoryFactory> logger,
        IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory) : base(
        mongoDbTransactionRepositoryFactory)
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

    public override async Task<IMongoSession> CreateSession(MongoTransactionSessionOptions options,
        CancellationToken cancellationToken)
    {
        var mongoSession = await base.CreateSession(options, cancellationToken);

        await AcquireLock(cancellationToken);

        if (_provisioned)
        {
            ReleaseLock();

            return mongoSession;
        }

        await mongoSession.MongoDatabase.Client.ProvisionCollections(mongoSession.MongoDatabase.DatabaseNamespace
            .DatabaseName, cancellationToken);

        _provisioned = true;

        _logger.LogInformation("MongoDb has been auto-provisioned");

        ReleaseLock();

        return mongoSession;
    }

    public static IMongoDbTransactionRepositoryFactory Create(IServiceProvider serviceProvider,
        IMongoDbTransactionRepositoryFactory mongoDbTransactionRepositoryFactory)
    {
        return ActivatorUtilities.CreateInstance<AutoProvisionMongoDbTransactionRepositoryFactory>(serviceProvider,
            mongoDbTransactionRepositoryFactory);
    }
}
