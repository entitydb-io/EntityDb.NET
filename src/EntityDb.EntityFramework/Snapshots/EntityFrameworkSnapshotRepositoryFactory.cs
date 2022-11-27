using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Snapshots;
using EntityDb.EntityFramework.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EntityDb.EntityFramework.Snapshots;

internal class EntityFrameworkSnapshotRepositoryFactory<TSnapshot, TDbContext> : DisposableResourceBaseClass,
    IEntityFrameworkSnapshotRepositoryFactory<TSnapshot>
    where TSnapshot : class
    where TDbContext : SnapshotReferenceDbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IOptionsFactory<EntityFrameworkSnapshotSessionOptions> _optionsFactory;

    public EntityFrameworkSnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IDbContextFactory<TDbContext> dbContextFactory,
        IOptionsFactory<EntityFrameworkSnapshotSessionOptions> optionsFactory
    )
    {
        _serviceProvider = serviceProvider;
        _dbContextFactory = dbContextFactory;
        _optionsFactory = optionsFactory;
    }

    public ISnapshotRepository<TSnapshot> CreateRepository(IEntityFrameworkSession<TSnapshot> entityFrameworkSession)
    {
        var entityFrameworkSnapshotRepository = new EntityFrameworkSnapshotRepository<TSnapshot>
        (
            entityFrameworkSession
        );

        return TryCatchSnapshotRepository<TSnapshot>.Create(_serviceProvider, entityFrameworkSnapshotRepository);
    }

    public async Task<IEntityFrameworkSession<TSnapshot>> CreateSession(EntityFrameworkSnapshotSessionOptions options, CancellationToken cancellationToken)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return EntityFrameworkSession<TSnapshot, TDbContext>.Create(_serviceProvider, dbContext, options);
    }

    public static EntityFrameworkSnapshotRepositoryFactory<TSnapshot, TDbContext> Create(IServiceProvider serviceProvider,
        string connectionString, string keyNamespace)
    {
        return ActivatorUtilities.CreateInstance<EntityFrameworkSnapshotRepositoryFactory<TSnapshot, TDbContext>>
        (
            serviceProvider,
            connectionString,
            keyNamespace
        );
    }

    public EntityFrameworkSnapshotSessionOptions GetTransactionSessionOptions(string snapshotSessionOptionsName)
    {
        return _optionsFactory.Create(snapshotSessionOptionsName);
    }
}
