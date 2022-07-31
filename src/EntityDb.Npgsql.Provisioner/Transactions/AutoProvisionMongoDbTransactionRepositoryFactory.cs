using EntityDb.Npgsql.Provisioner.Extensions;
using EntityDb.Npgsql.Queries;
using EntityDb.SqlDb.Sessions;
using EntityDb.SqlDb.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EntityDb.Npgsql.Provisioner.Transactions;

internal sealed class
    AutoProvisionNpgsqlTransactionRepositoryFactory : SqlDbTransactionRepositoryFactoryWrapper<NpgsqlQueryOptions>
{
    private static readonly SemaphoreSlim Lock = new(1);
    private static bool _provisioned;
    private readonly ILogger<AutoProvisionNpgsqlTransactionRepositoryFactory> _logger;

    public AutoProvisionNpgsqlTransactionRepositoryFactory
    (
        ILogger<AutoProvisionNpgsqlTransactionRepositoryFactory> logger,
        ISqlDbTransactionRepositoryFactory<NpgsqlQueryOptions> npgsqlTransactionRepositoryFactory) : base(npgsqlTransactionRepositoryFactory)
    {
        _logger = logger;
    }

    private async Task AcquireLock(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Wait for Npgsql Auto-Provisioning Lock");

        await Lock.WaitAsync(cancellationToken);

        _logger.LogInformation("Npgsql Auto-Provisioning Lock Acquired");
    }

    private void ReleaseLock()
    {
        _logger.LogInformation("Release Npgsql Auto-Provisioning Lock");

        Lock.Release();

        _logger.LogInformation("Npgsql Auto-Provisioning Lock Released");
    }

    public override async Task<ISqlDbSession<NpgsqlQueryOptions>> CreateSession(SqlDbTransactionSessionOptions options,
        CancellationToken cancellationToken)
    {
        var sqlDbSession = await base.CreateSession(options, cancellationToken);

        await AcquireLock(cancellationToken);

        if (_provisioned)
        {
            _logger.LogInformation("Npgsql already auto-provisioned.");

            ReleaseLock();

            return sqlDbSession;
        }

        var npgsqlConnection = new NpgsqlConnection(options.ConnectionString);

        await npgsqlConnection.ProvisionTables(cancellationToken);

        _provisioned = true;

        _logger.LogInformation("Npgsql has been auto-provisioned");

        ReleaseLock();

        return sqlDbSession;
    }

    public static ISqlDbTransactionRepositoryFactory<NpgsqlQueryOptions> Create(IServiceProvider serviceProvider,
        ISqlDbTransactionRepositoryFactory<NpgsqlQueryOptions> npgsqlTransactionRepositoryFactory)
    {
        return ActivatorUtilities.CreateInstance<AutoProvisionNpgsqlTransactionRepositoryFactory>(serviceProvider,
            npgsqlTransactionRepositoryFactory);
    }
}
